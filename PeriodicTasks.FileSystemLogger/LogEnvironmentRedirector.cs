using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITVComponents.Logging;
using PeriodicTasks.Events;

namespace PeriodicTasks.FileSystemLogger
{
    public class LogEnvironmentRedirector : PeriodicTasksEventInterceptor
    {
        public LogEnvironmentRedirector(PeriodicEnvironment environment) : base(environment)
        {
        }

        #region Overrides of PeriodicTasksEventInterceptor

        protected override void InterceptTaskStarts(PeriodicTask target, Dictionary<string, object> variables)
        {
            LogEnvironment.LogEvent($"{target.Name} has entered running state.",LogSeverity.Report);
        }

        protected override void InterceptTaskEnds(PeriodicTask target, Dictionary<string, object> variables)
        {
            LogEnvironment.LogEvent($"{target.Name} has entered idle state.", LogSeverity.Report);
        }

        protected override void InterceptTaskEndsWithError(PeriodicTask target, Dictionary<string, object> variables)
        {
            LogEnvironment.LogEvent($"{target.Name} has terminated due to an error.", LogSeverity.Report);
        }

        protected override void InterceptBeforeTaskStep(PeriodicTask target, TaskStep step, int stepId, int stepCount, Dictionary<string, object> variables)
        {
            LogEnvironment.LogEvent($"{target.Name}: Executing Step {stepId} of {stepCount}...", LogSeverity.Report);
        }

        protected override void InterceptAfterTaskStep(PeriodicTask target, TaskStep step, int stepId, int stepCount, object result,
            Dictionary<string, object> variables)
        {
            LogEnvironment.LogEvent($"{target.Name}: Step {stepId} of {stepCount} was evaluated by {step.StepWorkerName} and resulted to {result}.", LogSeverity.Report);
        }

        protected override void InterceptTaskMessage(PeriodicTask target, LogMessageType messageType, string message)
        {
            LogSeverity severity = messageType == LogMessageType.Report
                ? LogSeverity.Report
                : (messageType == LogMessageType.Warning ? LogSeverity.Warning : LogSeverity.Error);
            LogEnvironment.LogEvent($"{target.Name}: {message}",severity);
        }

        protected override void InterceptTaskTerminatesPlanned(PeriodicTask target)
        {
            LogEnvironment.LogEvent($"{target.Name} has terminated at this step because a condition for continuing did not apply.", LogSeverity.Report);
        }

        protected override void InterceptTaskTerminationDueToRunCondition(PeriodicTask target, TaskStep step, int stepId, int stepCount)
        {
            LogEnvironment.LogEvent($"{target.Name}: Step {stepId} of {stepCount} caused the task to terminate because of its RunCondition ({step.RunCondition}) which has resulted to false", LogSeverity.Report);
        }

        #endregion
    }
}
