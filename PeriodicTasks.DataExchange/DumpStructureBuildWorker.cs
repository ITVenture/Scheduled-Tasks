using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ITVComponents.DataExchange.Interfaces;
using ITVComponents.Plugins.SelfRegistration;
using ITVComponents.Settings;
using PeriodicTasks.Remote;

namespace PeriodicTasks.DataExchange
{
    public class DumpStructureBuildWorker:StepWorker
    {
        /// <summary>
        /// The DataCollector that is used to collect data that is required by this StructureBuild worker
        /// </summary>
        private IDataCollector collector;

        /// <summary>
        /// Initializes a new instance of the DumpStructureBuildWorker class
        /// </summary>
        /// <param name="collector">the collector that is used to collect data</param>
        /// <param name="registration">a registration callback that is used to register this object in the factory</param>
        public DumpStructureBuildWorker(IDataCollector collector)
            : base()
        {
            this.collector = collector;
            ConsumedSections = new[] {new JsonSectionDefinition {Name = "PeriodicTasks_DataExchange_DataExchangeConfiguration", SettingsType = typeof(DataExchangeConfig)}};
        }

        /// <summary>
        /// Instructs the Plugin to read the JsonSettings or to create a default instance if none is available
        /// </summary>
        public override void ReadSettings()
        {
            var tmp = DataExchangeConfig.Helper.StructureConfigurations;
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
            string configName = GetParameterValue<string>("StructureConfig", values, task);
            return collector.CollectData(command,
                                         DataExchangeConfig.Helper.StructureConfigurations[configName].Configuration,
                                         s =>
                                             {
                                                 task.Log(string.Format("Checking parameter {0}", s),
                                                          LogMessageType.Report);
                                                 object tmp = GetParameterValue<object>(s, values, task);
                                                 task.Log(string.Format("about to return {0}",tmp),LogMessageType.Report);
                                                 return tmp;
                                             });
        }

        public override WorkerDescription Describe()
        {
            var retVal = base.Describe();
            retVal.Description = "Creates a hierarchical structured dataset according to the provided configuration";
            retVal.CommandDescription = "The name of the StructureBuilder object that is used to build the target-dataset.";
            retVal.ExpectedParameters = new[]
            {
                new ParameterDescription
                {
                    Name = "StructureConfig",
                    Description = "The Target-Structuring configuration that must be used to build the return-value",
                    ExpectedTypeName = "String",
                    Required = true
                },
                new ParameterDescription
                {
                    Name = "[...?]",
                    Description = "Any parameters that are required by your configured Queries",
                    ExpectedTypeName = "[Any]",
                    Required = false
                }
            };
            retVal.ReturnType = "DynamicResult[]";
            retVal.ReturnDescription = "A Structured dataset that is built using the provided Structure-Configuration";
            return retVal;
        }
    }
}
