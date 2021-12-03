using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ITVComponents.DataAccess;
using ITVComponents.DataAccess.Parallel;
using ITVComponents.Plugins.SelfRegistration;
using ITVComponents.Scripting.CScript.Core;
using ITVComponents.Scripting.CScript.Helpers;
using PeriodicTasks.Remote;

namespace PeriodicTasks.DataExchange
{
    public class StructureProcessWorker : StepWorker
    {
        /// <summary>
        /// The connection buffer that is used to connect to the database
        /// </summary>
        private IConnectionBuffer database;

        /// <summary>
        /// Initializes a new instance of the StructureProcessWorker class
        /// </summary>
        /// <param name="database">a connection buffer that is used to connect to the database</param>
        /// <param name="registration">the registration callback</param>
        public StructureProcessWorker(IConnectionBuffer database)
            : base()
        {
            this.database = database;
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
            bool retVal = true;
            DynamicResult[] data = GetParameterValue<DynamicResult[]>("data", values, task);
            IDbWrapper db;
            using (database.AcquireConnection(false, out db))
            {
                Dictionary<string, object> vars = new Dictionary<string, object>();
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                List<IDbDataParameter> dataParams = new List<IDbDataParameter>();
                foreach (var tmp in values)
                {
                    if (tmp.Key.ParameterName != "data")
                    {
                        parameters.Add(tmp.Key.ParameterName, tmp.Value as string);
                    }
                }

                foreach (DynamicResult result in data)
                {
                    vars["current"] = result;
                    dataParams.Clear();
                    foreach (var param in parameters)
                    {
                        dataParams.Add(db.GetParameter(param.Key, ExpressionParser.Parse(param.Value, vars, a => { DefaultCallbacks.PrepareDefaultCallbacks(a.Scope, a.ReplSession); })));
                    }

                    try
                    {
                        db.ExecuteCommand(command, dataParams.ToArray());
                    }
                    catch (Exception ex)
                    {
                        task.Log(ex.ToString(), LogMessageType.Error);
                        retVal = false;
                        break;
                    }
                }
            }

            return retVal;
        }

        /// <summary>
        /// Describes this worker
        /// </summary>
        /// <returns>a descriptor for this worker</returns>
        public override WorkerDescription Describe()
        {
            var tmp = base.Describe();
            tmp.CommandDescription = "The Command to execute for each item of the provided recordset";
            tmp.Description = "Executes a command for each record of a provided recordset";
            tmp.ExpectedParameters = new ParameterDescription[]
                                         {
                                             new ParameterDescription
                                                 {
                                                     Name = "data",
                                                     Description =
                                                         "The recordset on which to process all items",
                                                     ExpectedTypeName = "DynamicResult[]",
                                                     Required = true
                                                 }
                                         };
            tmp.Remarks =
                "Provide further parameters to configure the executed command. Each provided parameter is expected to be an executable expression. You get access to the current record with the variable \"current\"";
            tmp.ReturnType = "Boolean";
            tmp.ReturnDescription = "a value indicating whether all records were successfully processed";
            return tmp;
        }
    }
}
