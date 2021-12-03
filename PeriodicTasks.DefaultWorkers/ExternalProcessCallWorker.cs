using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ITVComponents.CommandLineParser;
using ITVComponents.ExtendedFormatting;
using ITVComponents.Formatting;
using ITVComponents.Invokation;
using ITVComponents.Plugins.SelfRegistration;
using ITVComponents.Threading;
using PeriodicTasks.DefaultWorkers.RunArguments;
using PeriodicTasks.Remote;

namespace PeriodicTasks.DefaultWorkers
{
    public class ExternalProcessCallWorker:StepWorker
    {
        private ExternalProgramInvoker invoker;

        public override bool SupportAsync => true;

        /*private const string ProcessRegex =
            @"^""(?<program>([^\""\\]|\\\""|\\\\)+)""(?<arguments>([^\""\\]|\\\""|\\\\)+)""(@(?<targetDir>.*))?$";*/

        /// <summary>
        /// Initializes a new instance of the ExternalProcessCallWorker class
        /// </summary>
        /// <param name="registration">the Registration - Callback that is used to register this plugin in the plugin loader</param>
        public ExternalProcessCallWorker() : base()
        {
            invoker = new ExternalProgramInvoker();
        }

        protected override object RunTask(PeriodicTask task, string command, Dictionary<StepParameter, object> values)
        {
            task.Log("Should have used the Async call!", LogMessageType.Error);
            return AsyncHelpers.RunSync(async () => await RunTaskAsync(task, command, values));
        }

        /// <summary>
        /// Runs a step of a given task
        /// </summary>
        /// <param name="task">the task that owns the current step</param>
        /// <param name="command">the command that will be evaluated by this worker</param>
        /// <param name="values">the variables that hold results of previous steps</param>
        /// <returns>the result of the provided command</returns>
        protected async override Task<object> RunTaskAsync(PeriodicTask task, string command, Dictionary<StepParameter, object> values)
        {
            CommandLineParser cmd = new CommandLineParser(typeof(RunProcessArguments));
            RunProcessArguments args = new RunProcessArguments();
            cmd.Configure(command, args);
            string program = args.Program;
            string arguments = args.Arguments;
            string targetDir = args.ExecutionPath;
            int timeout = args.ExecutionTimeout ?? -1;
            int maxRetries = args.MaxRetryCount ?? 0;
            bool showHelp = args.ShowHelp;
            if (!showHelp)
            {
                if (args.FormatArguments || args.FormatProgram)
                {
                    Dictionary<string, object> vals = values.ToDictionary(tmp => tmp.Key.ParameterName, tmp => tmp.Value);
                    program = args.FormatProgram ? vals.FormatText(program) : program;
                    arguments = args.FormatArguments ? args.FormatText(arguments) : arguments;
                }

                task.Log(
                    string.Format("Executing {0}. Arguments: {1}, TargetDirectory: {2}", program, arguments, targetDir),
                    LogMessageType.Report);
                var retVal = await invoker.RunApplicationAsync(program, targetDir, arguments, timeout, maxRetries);
                task.Log(string.Format(@"Exit-Code: {0}
Console-Output: {1}
Error-Output: {2}", retVal.ExitCode, retVal.ConsoleOutput, retVal.ErrorOutput), LogMessageType.Report);
                return retVal;
            }

            task.Log(cmd.PrintUsage(false,0,100), LogMessageType.Report);
            return null;
        }

        public override WorkerDescription Describe()
        {
            CommandLineParser cmd = new CommandLineParser(typeof(RunProcessArguments));
            var retVal = base.Describe();
            retVal.Description =
                "Runs a configurable Application and returns the ExitCode, the ConsoleOutput and the ErrorOutput of it.";
            retVal.CommandDescription = cmd.PrintUsage(false, 0, 100);
            retVal.ReturnType = "ProgramTerminationInformation";
            retVal.ReturnDescription = "All Information that was provided by the executed application";
            return retVal;
        }
    }
}
