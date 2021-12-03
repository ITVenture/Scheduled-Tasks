using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ITVComponents.CommandLineParser;

namespace PeriodicTasks.DefaultWorkers.RunArguments
{
    internal class SqlProcessArguments
    {
        [CommandParameter(ArgumentName="/mode:", ParameterDescription="The SQL execution mode for this query")]
        public SqlQueryWorker.SqlModes Mode { get; set; }

        [CommandParameter(ArgumentName="/query:", ParameterDescription="The SQL Command to execute on the target server")]
        public string Query { get; set; }

        [CommandParameter(ArgumentName="/RunCondition:", ParameterDescription="The Condition that must be true to run the Query", IsOptional = true)]
        public string Condition { get; set; }

        [CommandParameter(ArgumentName = "/?", IsHelpParameter = true, ParameterDescription = "Displays this Help-Message in the Task-Log")]
        public bool ShowHelp { get; set; }
    }
}
