using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using ITVComponents.Threading;
using PeriodicTasks.Events;

namespace PeriodicTasks.FileSystemLogger
{
    public abstract class FileLog:PeriodicTasksEventInterceptor
    {
        /// <summary>
        /// twelve hours in milliseconds
        /// </summary>
        private const int TwelveHours = 43200000;

        /// <summary>
        /// the logging directory that is used to log progress on tasks
        /// </summary>
        private string logDirectory;

        /// <summary>
        /// the number of days to keep log-files before deletion
        /// </summary>
        private int fileRetention;

        /// <summary>
        /// Initializes a new instance of the FileLog class
        /// </summary>
        /// <param name="environment">the periodic environment that is used to execute tasks</param>
        /// <param name="logDirectory">the target directory where logs are being saved</param>
        /// <param name="fileRetention">the number of days to keep log-files before deletion</param>
        protected FileLog(PeriodicEnvironment environment, string logDirectory, int fileRetention) : base(environment)
        {
            this.fileRetention = fileRetention;
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            this.logDirectory = logDirectory;
            BackgroundRunner.AddPeriodicTask(RunHousekeep, TwelveHours);
        }

        /// <summary>
        /// Führt anwendungsspezifische Aufgaben durch, die mit der Freigabe, der Zurückgabe oder dem Zurücksetzen von nicht verwalteten Ressourcen zusammenhängen.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public override void Dispose()
        {
            BackgroundRunner.RemovePeriodicTask(RunHousekeep);
            base.Dispose();
        }

        /// <summary>
        /// Intercepts the Task-Started event on the provided task
        /// </summary>
        /// <param name="target">the target task that has started</param>
        /// <param name="variables">the variables of the given task</param>
        protected override void InterceptTaskStarts(PeriodicTask target, Dictionary<string, object> variables)
        {
            var writer = GetWriter(target);
            writer.WriteLine(@"Report   {0:dd.MM.yyyy HH:mm:ss}: {1} has entered running state.", DateTime.Now, target.Name);
        }

        /// <summary>
        /// Intercepts the Task-Ended event on the provided task
        /// </summary>
        /// <param name="target">the target task that has ended</param>
        /// <param name="variables">the variables of the given task</param>
        protected override void InterceptTaskEnds(PeriodicTask target, Dictionary<string, object> variables)
        {
            if (!DisposeWriter(target))
            {
                //Console.WriteLine("Warning: Unable to properly end Logging for this Task!");
            }
        }

        /// <summary>
        /// Intercepts the Task-Ended event on the provided task
        /// </summary>
        /// <param name="target">the target task that has ended</param>
        /// <param name="variables">the variables of the given task</param>
        protected override void InterceptTaskEndsWithError(PeriodicTask target, Dictionary<string, object> variables)
        {
            InterceptTaskMessage(target, LogMessageType.Error, "Task has ended with errors.");
        }

        /// <summary>
        /// Intercepts the event before-step on the provided task
        /// </summary>
        /// <param name="target">the task on which a step is about to be executed</param>
        /// <param name="step">the step that is executing</param>
        /// <param name="stepId">the sequence-identity (1..n) of the current step</param>
        /// <param name="stepCount">the total number of steps on the current task</param>
        /// <param name="variables">the variables for the given job</param>
        protected override void InterceptBeforeTaskStep(PeriodicTask target, TaskStep step, int stepId, int stepCount, Dictionary<string, object> variables)
        {

            LogMessage(target, string.Format("Executing Step {0} of {1}...", stepId, stepCount),
                       LogMessageType.Report);
        }

        /// <summary>
        /// Intercepts the event after-step on the provided task
        /// </summary>
        /// <param name="target">the task that has finished a specific step</param>
        /// <param name="step">the step that has finished execution</param>
        /// <param name="stepId">the sequence-identity (1..n) of the current step</param>
        /// <param name="stepCount">the total number of steps on the current task</param>
        /// <param name="result">the result that was returned by the worker of the given step</param>
        /// <param name="variables">the variables of the given job</param>
        protected override void InterceptAfterTaskStep(PeriodicTask target, TaskStep step, int stepId, int stepCount, object result, Dictionary<string, object> variables)
        {
            LogMessage(target,
                       string.Format("Step was evaluated by {0} and resulted to {1}", step.StepWorkerName, result),
                       LogMessageType.Report);
        }

        /// <summary>
        /// Intercepts the event when a job tries to log a message
        /// </summary>
        /// <param name="target">the target job that has generated a message</param>
        /// <param name="messageType">the type of the message</param>
        /// <param name="message">the message that was generated</param>
        protected override void InterceptTaskMessage(PeriodicTask target, LogMessageType messageType, string message)
        {
            LogMessage(target, message, messageType);
        }

        /// <summary>
        /// Intercepts the event when a job terminates because of a condition that did not apply
        /// </summary>
        /// <param name="target">the target job that has generated a message</param>
        protected override void InterceptTaskTerminatesPlanned(PeriodicTask target)
        {
            LogMessage(target,
                       "Task has terminated at this step because a condition for continuing did not apply",
                       LogMessageType.Report);
        }

        /// <summary>
        /// Intercepts the event when a job terminates because of the RunCondition of a Step that resulted to false
        /// </summary>
        /// <param name="target">the target job that has generated a message</param>
        /// <param name="step">the step that has caused to termination of a Task execution</param>
        /// <param name="stepId">the sequence-identity (1..n) of the current step</param>
        /// <param name="stepCount">the total number of steps on the current task</param>
        protected override void InterceptTaskTerminationDueToRunCondition(PeriodicTask target, TaskStep step, int stepId,
            int stepCount)
        {
            InterceptTaskMessage(target, LogMessageType.Report,
                string.Format(
                    "Step {0} of {1} ({2}) caused the Task to Terminate, because of its RunCondition ({3}) which resulted to false",
                    stepId, stepCount, step.StepName, step.RunCondition));
        }

        /// <summary>
        /// Gets a LogWriter for a specific task
        /// </summary>
        /// <param name="target">the target task for which to get the logWriter</param>
        /// <param name="logDirectory">the directory into which the demanded file is created</param>
        /// <returns>a TextWriter instance that points to an appropriate file</returns>
        protected abstract TextWriter CreateWriter(PeriodicTask target, string logDirectory);

        /// <summary>
        /// Runs a Housekeep job on the FileSystem
        /// </summary>
        /// <param name="logDirectory">the directory that holds all log-files</param>
        /// <param name="fileRetention">the number of days to keep log-files before deletion</param>
        protected abstract void HousekeepLogs(string logDirectory, int fileRetention);

        /// <summary>
        /// Gets a LogWriter for a specific task
        /// </summary>
        /// <param name="target">the target task for which to get the logWriter</param>
        /// <returns>a TextWriter instance that points to an appropriate file</returns>
        private TextWriter GetWriter(PeriodicTask target)
        {
            var writer = target.GetTemporaryValue<TextWriter>("TaskWriter");
            if (writer == null)
            {
                target.SetTemporaryValue("TaskWriter", writer = CreateWriter(target, logDirectory));
            }

            return writer;
        }

        /// <summary>
        /// Runs the housekeep - job on this object
        /// </summary>
        private void RunHousekeep()
        {
            HousekeepLogs(logDirectory, fileRetention);
        }

        /// <summary>
        /// Disposes the writer for a specific target
        /// </summary>
        /// <param name="target">the target task for which to dispose the writer</param>
        /// <returns>a value indicating whether the writer was initialized for the given task</returns>
        private bool DisposeWriter(PeriodicTask target)
        {
                var writer = target.GetTemporaryValue<TextWriter>("TaskWriter");
                if (writer != null)
                {
                    writer.WriteLine(@"Report   {0:dd.MM.yyyy HH:mm:ss}: {1} has entered idle state.", DateTime.Now, target.Name);
                    writer.Dispose();
                    return true;
                }

                return false;
        }

        /// <summary>
        /// Writes a log message
        /// </summary>
        /// <param name="target">the task for which to log a message</param>
        /// <param name="message">the message that is provided by a specific step of a task</param>
        /// <param name="type">the type of the message</param>
        /// <returns>indicates whether the log was written successfully</returns>
        private void LogMessage(PeriodicTask target, string message, LogMessageType type)
        {
            var writer = GetWriter(target);
            writer.WriteLine("{0}   {1:dd.MM.yyyy HH:mm:ss}: {2}", type, DateTime.Now, message);
        }
    }
}
