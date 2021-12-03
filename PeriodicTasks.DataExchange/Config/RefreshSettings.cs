using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ITVComponents.CommandLineParser;

namespace PeriodicTasks.DataExchange.Config
{
    public class RefreshSettings
    {
        [CommandParameter(ArgumentName="/Source:", IsOptional=false, IsHelpParameter=false, ParameterDescription="The Parameter that provides the data that must be added to the target collection") ]
        public string SourceParameter { get; set; }

        [CommandParameter(ArgumentName = "/Target:", IsOptional = false, IsHelpParameter = false, ParameterDescription = "The registration name of the target collection to which to copy the content of the source collection")]
        public string TargetCollection { get; set; }

        [CommandParameter(ArgumentName = "/Help", IsOptional = true, IsHelpParameter = true, ParameterDescription = "Displays this Help message")]
        public bool ShowHelp { get; set; }
    }
}
