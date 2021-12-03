using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Interop;
using ITVComponents.CommandLineParser;
using ITVComponents.DataAccess;
using ITVComponents.DataExchange.Interfaces;
using ITVComponents.Plugins.SelfRegistration;
using ITVComponents.Scripting.CScript.Core;
using ITVComponents.Scripting.CScript.Helpers;
using ITVComponents.Settings;
using PeriodicTasks.DataExchange.Config;
using PeriodicTasks.Remote;

namespace PeriodicTasks.DataExchange
{
    public class StructureDumpWorker:StepWorker
    {
        /// <summary>
        /// A DataDumper object that is used to export the source data
        /// </summary>
        private IDataDumper dataDumper;

        /// <summary>
        /// the commandline parser that is used to configure this worker
        /// </summary>
        private CommandLineParser configurator;

        /// <summary>
        /// Initializes a new instance of the StructureDumpWorker class
        /// </summary>
        /// <param name="dataDumper">the data dumper that is used to dump records</param>
        /// <param name="registration">a registration callback that is used to register this worker in the factory</param>
        public StructureDumpWorker(IDataDumper dataDumper)
            : base()
        {
            configurator = new CommandLineParser(typeof(DumpConfig));
            this.dataDumper = dataDumper;
            ConsumedSections = new[] {new JsonSectionDefinition {Name = "PeriodicTasks_DataExchange_DataExchangeConfiguration", SettingsType = typeof(DataExchangeConfig)}};
        }

        /// <summary>
        /// Instructs the Plugin to read the JsonSettings or to create a default instance if none is available
        /// </summary>
        public override void ReadSettings()
        {
            var tmp = DataExchangeConfig.Helper.DumpConfigurations;
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
            DumpConfig cfg = new DumpConfig();
            configurator.Configure(command, cfg);
            if (!cfg.ShowHelp)
            {
                var dcf = DataExchangeConfig.Helper.DumpConfigurations[cfg.DumpConfigName];
                var data = GetParameterValue<DynamicResult[]>(cfg.SourceVariable, values, task);
                if (!string.IsNullOrEmpty(cfg.TargetFile))
                {
                    Dictionary<string, object> variables = new Dictionary<string, object>();
                    foreach (KeyValuePair<StepParameter, object> val in values)
                    {
                        variables.Add(val.Key.ParameterName, val.Value);
                    }

                    string fileName = ExpressionParser.Parse(cfg.TargetFile, variables,
                        a => { DefaultCallbacks.PrepareDefaultCallbacks(a.Scope, a.ReplSession); }) as string;
                    if (dataDumper.DumpData(fileName,
                        data,
                        dcf))
                    {
                        return fileName;
                    }

                    if (fileName != null && File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                }
                else
                {
                    byte[] ret = null;
                    var mst = new MemoryStream();
                    try
                    {
                        dataDumper.DumpData(mst, data, dcf);
                    }
                    finally
                    {
                        mst.Flush();
                        mst.Close();
                        ret = mst.ToArray();
                    }

                    return ret;
                }

                return null;
            }

            task.Log(configurator.PrintUsage(false, 0, 100),LogMessageType.Report);
            return true;
        }

        public override WorkerDescription Describe()
        {
            var retVal = base.Describe();
            retVal.Description = "Dumps a Structured Data-Collection to a file using configured output formatting and constants.";
            retVal.CommandDescription = configurator.PrintUsage(false, 0, 100);
            retVal.ExpectedParameters = new[]
            {
                new ParameterDescription
                {
                    Name = "[/Source]",
                    Description = "The Source collection that must be dumped into a file",
                    ExpectedTypeName = "DynamicResult[]",
                    Required = true
                },
                new ParameterDescription
                {
                    Name = "[...?]",
                    Description = "Any parameter that is useful for your Target-File expression"
                }
            };

            retVal.ReturnType = "NULL";
            retVal.ReturnDescription = "No Return value";
            return retVal;
        }
    }
}
