using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ITVComponents.CommandLineParser;
using ITVComponents.DataAccess;
using ITVComponents.DataAccess.Parallel;
using ITVComponents.Plugins;
using ITVComponents.Plugins.SelfRegistration;
using PeriodicTasks.DefaultWorkers.RunArguments;
using PeriodicTasks.Remote;

namespace PeriodicTasks.DefaultWorkers
{
    public class SqlQueryWorker:StepWorker
    {
        /// <summary>
        /// The database connection that is used to access data
        /// </summary>
        private IConnectionBuffer database;

        private readonly PluginFactory factory;

        private readonly bool registerTaskAsLocal;

        /// <summary>
        /// Initializes a new instance of the SqlQueryWorker class
        /// </summary>
        /// <param name="database">the database connection dealing with the required data</param>
        /// <param name="registration">the callback that is used to register this component in the factory</param>
        public SqlQueryWorker(IConnectionBuffer database) : base()
        {
            this.database = database;
        }

        /// <summary>
        /// Initializes a new instance of the SqlQueryWorker class
        /// </summary>
        /// <param name="database">the database connection dealing with the required data</param>
        /// <param name="registration">the callback that is used to register this component in the factory</param>
        /// <param name="registerTaskAsLocal">indicates whether to add a temporary registration to the PlugInFactory before loading the connection</param>
        public SqlQueryWorker(IConnectionBuffer database, PluginFactory factory, bool registerTaskAsLocal) : base()
        {
            this.database = database;
            this.factory = factory;
            this.registerTaskAsLocal = registerTaskAsLocal;
        }

        /// <summary>
        /// Runs a step of a given task
        /// </summary>
        /// <param name="task">the task that owns the current step</param>
        /// <param name="command">the command that will be evaluated by this worker</param>
        /// <param name="values">the variables that hold results of previous steps</param>
        /// <returns>the result of the provided command</returns>
        protected override object RunTask(PeriodicTask task, string command, Dictionary<StepParameter, object> values)
        {
            SqlProcessArguments args = new SqlProcessArguments();
            CommandLineParser parser = new CommandLineParser(typeof (SqlProcessArguments));
            parser.Configure(command, args);
            string cmd = args.Query;
            SqlModes mode = args.Mode;
            bool hasCondition = !string.IsNullOrEmpty(args.Condition);
            
            if (!args.ShowHelp && (!hasCondition || GetParameterValue<bool>(args.Condition,values,task)))
            {
                task.Log(string.Format("Executing the command {0} on Target {1}", cmd, mode), LogMessageType.Report);
                IDbWrapper connection;
                if (registerTaskAsLocal)
                {
                    factory.RegisterObjectLocal("task", task);
                }

                try
                {
                    using (database.AcquireConnection(false, out connection))
                    {
                        IDbDataParameter[] parameters = (from t in values
                                where (!hasCondition || (t.Key.ParameterName != args.Condition))
                                select
                                    CreateParameter(connection, t.Key, t.Value))
                            .ToArray();
                        switch (mode)
                        {
                            case SqlModes.Scalar:
                            {
                                return connection.ExecuteCommandScalar<object>(cmd,
                                    parameters);
                            }
                            case SqlModes.Proc:
                            {
                                return connection.CallProcedure(cmd, null, parameters);
                            }
                            case SqlModes.Complex:
                            {
                                return connection.ExecuteComplexCommand(cmd, parameters);
                            }
                            case SqlModes.Records:
                            {
                                return connection.GetNativeResults(cmd, null, parameters);
                            }
                        }
                    }
                }
                finally
                {
                    if (registerTaskAsLocal)
                    {
                        factory.ClearLocalRegistrations();
                    }
                }
            }
            if (args.ShowHelp)
            {
                task.Log(parser.PrintUsage(false, 0, 100), LogMessageType.Report);
            }

            return null;
        }

        /// <summary>
        /// Describes this worker
        /// </summary>
        /// <returns>a descriptor for this worker</returns>
        public override WorkerDescription Describe()
        {
            CommandLineParser cmd = new CommandLineParser(typeof (SqlProcessArguments));
            var retVal = base.Describe();
            retVal.Description = "Runs an SQL - Command against a configured DataSource.";
            retVal.Remarks = string.Format(@"Connected to {0}. 
Use the mode Parameter as:
Scalar:  reading only the first Column of the first Result of your command (return-type: object)
Proc:    calling a procedure that returns one or more Data-Sets (return-type: List<DynamicResult[]>)
Records: reading the complete resultset (only the first) of the provided command (return-type: DynamicResult[])
Complex: Executes a complex command and returns the result of it. Use this if you run multiple commands as in a stored procedure. (return-type: int)

If you need Parameters in your Command provide them as Step-Parameters", database.UniqueName);
            retVal.CommandDescription = cmd.PrintUsage(false, 0, 100);
            retVal.ReturnType = "Depends on the mode. (see remarks)";
            retVal.ReturnDescription = "The result of the executed command";
            return retVal;
        }

        /// <summary>
        /// Creates database parameters for a given step - parameter and its value
        /// </summary>
        /// <param name="connection">the current database connection</param>
        /// <param name="parameter">the underlaying step parameter</param>
        /// <param name="value">the value provided to it</param>
        /// <returns>an IDbDataParameter representing the requested value</returns>
        private IDbDataParameter CreateParameter(IDbWrapper connection, StepParameter parameter, object value)
        {
            return connection.GetParameter(parameter.ParameterName, value,
                                         !string.IsNullOrEmpty(parameter.ParameterSettings)
                                             ? parameter.ParameterSettings
                                             : null);
        }

        /// <summary>
        /// Specifies possible modes of sql execution calls
        /// </summary>
        public enum SqlModes
        {
            /// <summary>
            /// Makes a call that expects only a scalar value
            /// </summary>
            Scalar,

            /// <summary>
            /// Calls a procedure and expects one or multiple datasets to be returned
            /// </summary>
            Proc,

            /// <summary>
            /// Retrieves a recordset using a select statement
            /// </summary>
            Records,

            /// <summary>
            /// Executes a Complex command (code fragment as in a stored procedure)
            /// </summary>
            Complex
        }
    }
}
