using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ITVComponents.CommandLineParser;

namespace PeriodicTasks.DefaultWorkers.RunArguments
{
    internal class RunProcessArguments
    {
        [CommandParameter(ArgumentName = "/program:", ParameterDescription = "The Application that will be invoked")]
        public string Program { get; set; }

        [CommandParameter(ArgumentName = "/arguments:", IsOptional = true,
            ParameterDescription = "The arguments that will be passed to the Target-Application")]
        public string Arguments { get; set; }

        [CommandParameter(ArgumentName = "/executionPath:", IsOptional = true,
            ParameterDescription = "The Home-Directory for the task execution")]
        public string ExecutionPath { get; set; }

        [CommandParameter(ArgumentName = "/timeout:", IsOptional = true,
            ParameterDescription = "The execution timeout in which the application must have terminated")]
        public int? ExecutionTimeout { get; set; }

        [CommandParameter(ArgumentName = "/retryCount:", IsOptional = true,
            ParameterDescription = "The maximum count of retries that can be taken before call is considered as failed")
        ]
        public int? MaxRetryCount { get; set; }

        [CommandParameter(ArgumentName = "/formatProgram",
            ParameterDescription = "Indicates whether to format the program using the provided step-arguments")]
        public bool FormatProgram { get; set; }

        [CommandParameter(ArgumentName = "/formatArguments",
            ParameterDescription = "Indicates whether to format the arguments using the provided step-arguments")]
        public bool FormatArguments { get; set; }

        [CommandParameter(ArgumentName = "/?", ParameterDescription = "Displays this Help-Message in the Task-Log")]
        public bool ShowHelp { get; set; }
    }
}
