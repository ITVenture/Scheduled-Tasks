using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITVComponents.CommandLineParser;

namespace PeriodicTasks.Exchange.Config
{
    public class AttachmentWorkerArguments
    {
        [CommandParameter(ArgumentName="/ExchangeConfig:",IsHelpParameter=false, IsOptional=false, ParameterDescription = "The Configuration to use for accessing Exchange Server")]
        public string ExchangeConfig { get; set; }

        [CommandParameter(ArgumentName="/AttachmentConfig:", IsHelpParameter=false, IsOptional = false, ParameterDescription="The Configuration to use for Downloading Attachments")]
        public string AttachmentConfig { get; set; }

        [CommandParameter(ArgumentName="/?",IsHelpParameter=true, IsOptional=true, ParameterDescription="Displays this Help-Message in the Task-Log")]
        public bool ShowHelp { get; set; }
    }
}
