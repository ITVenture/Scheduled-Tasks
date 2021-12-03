using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ITVComponents.CommandLineParser;

namespace PeriodicTasks.DataExchange.Config
{
    public class DumpConfig
    {
        [CommandParameter(ArgumentName="/ConfigName:",IsOptional=false,ParameterDescription="The name of the DumpConfiguration for this step")]
        public string DumpConfigName { get; set; }

        [CommandParameter(ArgumentName="/Source:",IsOptional=false, ParameterDescription = "The name of the Variable containing the source-data for this dump")]
        public string SourceVariable { get; set; }

        [CommandParameter(ArgumentName="/TargetFile:",IsOptional=true,ParameterDescription="The file-Path for the targetfile that is being written. If this parameter is omitted, a byte-array containing the dumped data is returned.")]
        public string TargetFile { get; set; }

        [CommandParameter(ArgumentName="/Help", IsHelpParameter=true,IsOptional=true, ParameterDescription="Displays this Help information")]
        public bool ShowHelp { get; set; }
    }
}
