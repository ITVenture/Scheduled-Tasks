using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ITVComponents.Plugins;
using ITVComponents.Plugins.SelfRegistration;
using ITVComponents.Scripting.CScript;
using ITVComponents.Scripting.CScript.Helpers;
using PeriodicTasks.Remote;

namespace PeriodicTasks.DefaultWorkers
{
    public class CScriptWorker:StepWorker
    {
        private PluginFactory factory;

        /// <summary>
        /// Initializes a new instance of the XmlScriptWorker class
        /// </summary>
        /// <param name="factory">the PluginFactory object that is used to provide scripts with loaded plugin instances</param>
        /// <param name="registration">the registration callback that is used to register this plugin in the factory</param>
        public CScriptWorker(PluginFactory factory)
            : base()
        {
            this.factory = factory;
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
            ScriptFile<object> runner = ScriptFile<object>.FromFile(command);
            Dictionary<string, object> variables = new Dictionary<string, object>();
            foreach (KeyValuePair<StepParameter, object> value in values)
            {
                variables.Add(value.Key.ParameterName, GetParameterValue<object>(value.Key.ParameterName, values, task));
            }

            variables["Require"] = new Func<string, object>(n => factory[n]);
            variables["LogMessageType"] = typeof(LogMessageType);
            variables["Log"] = new Action<string, LogMessageType>(task.Log);
            return runner.Execute(variables, n => DefaultCallbacks.PrepareDefaultCallbacks(n.Scope, n.ReplSession));
        }

        /// <summary>
        /// Describes this worker
        /// </summary>
        /// <returns>a descriptor for this worker</returns>
        public override WorkerDescription Describe()
        {
            var retVal = base.Describe();
            retVal.Description = "Runs a C - Script";
            retVal.Remarks = @"Provide Variables, you want to use in the script as Step-Parameters.
Use the Require - Command to get an instance of a loaded Plugin by its uniqueName (i.e.: connection = Require(""SqlConnection"");)";
            retVal.CommandDescription = "The complete Path to the Script, you want to run.";
            retVal.ReturnType = "object";
            retVal.ReturnType = "The result of the executed script";
            return retVal;
        }
    }
}
