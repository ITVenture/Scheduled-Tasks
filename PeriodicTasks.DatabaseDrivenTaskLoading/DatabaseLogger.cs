using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ITVComponents.DataAccess;
using ITVComponents.DataAccess.Parallel;
using ITVComponents.Logging;
using PeriodicTasks.DatabaseDrivenTaskLoading.RecordExtensions;
using PeriodicTasks.Events;

namespace PeriodicTasks.DatabaseDrivenTaskLoading
{
    public class DatabaseLogger:PeriodicTasksEventInterceptor
    {
        /// <summary>
        /// Holds a connection buffer that is capable for logging events into a database
        /// </summary>
        private DatabaseConnectionBuffer connector;

        /// <summary>
        /// Holds the current Steps of the given tasks
        /// </summary>
        private Dictionary<int, TaskStep> currentSteps; 

        /// <summary>
        /// Initializes a new instance of the DatabaseLogger class
        /// </summary>
        /// <param name="environment">the Environment that executes the tasks that are logged by this instance</param>
        /// <param name="connector">the database connector that is used to log events</param>
        public DatabaseLogger(PeriodicEnvironment environment, DatabaseConnectionBuffer connector) : base(environment)
        {
            currentSteps = new Dictionary<int, TaskStep>();
            this.connector = connector;
        }

        /// <summary>
        /// Intercepts the Task-Started event on the provided task
        /// </summary>
        /// <param name="target">the target task that has started</param>
        /// <param name="variables">the variables of the given task</param>
        protected override void InterceptTaskStarts(PeriodicTask target, Dictionary<string, object> variables)
        {
            IDbWrapper database;
            using (connector.AcquireConnection(false, out database))
            {
                int runId = GetRunForTask(target, database);
                if (runId != -1)
                {
                    LogMessageOnRun(runId, null, string.Format("{0} has entered running state", target.Name),
                                    LogMessageType.Report, database);
                }
            }
        }

        /// <summary>
        /// Intercepts the Task-Ended event on the provided task
        /// </summary>
        /// <param name="target">the target task that has ended</param>
        /// <param name="variables">the variables of the given task</param>
        protected override void InterceptTaskEnds(PeriodicTask target, Dictionary<string, object> variables)
        {
            IDbWrapper database;
            using (connector.AcquireConnection(false, out database))
            {
                int runId = GetRunForTask(target, database);
                if (runId != -1)
                {
                    LogMessageOnRun(runId, null, string.Format("{0} has entered idle state", target.Name),
                                    LogMessageType.Report, database);
                    if (!FinishRun(target, runId, database))
                    {
                        LogEnvironment.LogDebugEvent("Unable to finish this run properly",LogSeverity.Warning);
                    }
                }
            }
        }

        protected override void InterceptTaskEndsWithError(PeriodicTask target, Dictionary<string, object> variables)
        {
            IDbWrapper database;
            using (connector.AcquireConnection(false, out database))
            {
                int runId = GetRunForTask(target, database);
                if (runId != -1)
                {
                    LogMessageOnRun(runId, null, string.Format("{0} has terminated due to an error.", target.Name),
                        LogMessageType.Error, database);
                }
            }
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
            IDbWrapper database;
            using (connector.AcquireConnection(false, out database))
            {
                int runId = GetRunForTask(target, database);
                if (runId != -1)
                {
                    LogMessageOnRun(runId, step, string.Format("Executing Step {0} of {1}...", stepId,stepCount), LogMessageType.Report, database);
                    lock (currentSteps)
                    {
                        currentSteps[runId] = step;
                    }
                }
            }
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
        protected override void InterceptAfterTaskStep(PeriodicTask target, TaskStep step, int stepId, int stepCount, object result,
                                                       Dictionary<string, object> variables)
        {
            IDbWrapper database;
            using (connector.AcquireConnection(false, out database))
            {
                int runId = GetRunForTask(target, database);
                if (runId != -1)
                {
                    LogMessageOnRun(runId, step, string.Format("Step was evaluated by {0} and resulted to {1}", step.StepWorkerName, result), LogMessageType.Report, database);
                    lock (currentSteps)
                    {
                        currentSteps.Remove(runId);
                    }
                }
            }
        }

        /// <summary>
        /// Intercepts the event when a job tries to log a message
        /// </summary>
        /// <param name="target">the target job that has generated a message</param>
        /// <param name="messageType">the type of the message</param>
        /// <param name="message">the message that was generated</param>
        protected override void InterceptTaskMessage(PeriodicTask target, LogMessageType messageType, string message)
        {
            IDbWrapper database;
            using (connector.AcquireConnection(false, out database))
            {
                int runId = GetRunForTask(target, database);
                if (runId != -1)
                {
                    TaskStep step = null;
                    lock (currentSteps)
                    {
                        if (currentSteps.ContainsKey(runId))
                        {
                            step = currentSteps[runId];
                        }
                    }

                    LogMessageOnRun(runId, step, message, messageType, database);
                }
            }
        }

        /// <summary>
        /// Intercepts the event when a job terminates because of a condition that did not apply
        /// </summary>
        /// <param name="target">the target job that has generated a message</param>
        protected override void InterceptTaskTerminatesPlanned(PeriodicTask target)
        {
            InterceptTaskMessage(target, LogMessageType.Report,
                                 "Task has terminated at this step because a condition for continuing did not apply");
        }

        /// <summary>
        /// Intercepts the event when a job terminates because of the RunCondition of a Step that resulted to false
        /// </summary>
        /// <param name="target">the target job that has generated a message</param>
        /// <param name="step">the step that has caused to termination of a Task execution</param>
        /// <param name="stepId">the sequence-identity (1..n) of the current step</param>
        /// <param name="stepCount">the total number of steps on the current task</param>
        protected override void InterceptTaskTerminationDueToRunCondition(PeriodicTask target, TaskStep step, int stepId, int stepCount)
        {
            InterceptTaskMessage(target, LogMessageType.Report,
                string.Format(
                    "Step {0} of {1} ({2}) caused the Task to Terminate, because of its RunCondition ({3}) which resulted to false",
                    stepId, stepCount, step.StepName, step.RunCondition));
        }

        /// <summary>
        /// Gets a current run for the given task
        /// </summary>
        /// <param name="task">the task that is currently running and for which a message must be logged</param>
        /// <param name="database">the database into which a new run must be added</param>
        /// <returns>the identity of the current run of the given task</returns>
        private int GetRunForTask(PeriodicTask task, IDbWrapper database)
        {
            object id = task.ReadMetaInformation("periodicTaskId");
            if (id is int taskId)
            {
                var runId = task.GetTemporaryValue<int>("CurrentRunId");
                LogEnvironment.LogDebugEvent($"CurrentRunId before Check: {runId}", LogSeverity.Report);
                if (runId == 0)
                {
                    IDbDataParameter taskIdParam = database.GetParameter("periodicTaskId", taskId);
                    IDbDataParameter startTimeParam = database.GetParameter("startTime", DateTime.Now);
                    runId =
                        database.ExecuteCommandScalar<int>(
                            @"Insert into PeriodicRuns (periodicTaskId, startTime) values (@periodicTaskId, @startTime);
select scope_identity()", taskIdParam, startTimeParam);
                    task.SetTemporaryValue("CurrentRunId", runId);
                }

                LogEnvironment.LogDebugEvent($"CurrentRunId before return: {runId}", LogSeverity.Report);
                return runId;
            }

            LogEnvironment.LogDebugEvent("Can only Log Tasks that were loaded from a database", LogSeverity.Warning);
            return -1;
        }

        /// <summary>
        /// Logs a message for the provided run in the provided step
        /// </summary>
        /// <param name="runId">the id of the current run</param>
        /// <param name="step">the id of the step that is currently running</param>
        /// <param name="message">the message to log</param>
        /// <param name="type">the Log-Type for the message</param>
        /// <param name="database">the database into which to log the event</param>
        private void LogMessageOnRun(int runId, TaskStep step, string message, LogMessageType type, IDbWrapper database)
        {
            DbTaskStep dstep = step as DbTaskStep;
            if (dstep == null && step != null)
            {
                throw new InvalidOperationException("Step must be of Type DbTaskStep if provided!");
            }

            IDbDataParameter runIdParam = database.GetParameter("periodicRunId", runId);
            IDbDataParameter stepIdParam = database.GetParameter("periodicStepId",
                                                                 dstep != null
                                                                     ? (object) dstep.TaskStepId
                                                                     : DBNull.Value);
            IDbDataParameter messageParam = database.GetParameter("message", message);
            IDbDataParameter typeParam = database.GetParameter("messageType", (int) type);
            IDbDataParameter logTimeParam = database.GetParameter("logTime", DateTime.Now);
            database.ExecuteCommand(
                "Insert into periodicLog (periodicRunId, periodicStepId, message, messageType, logTime) values (@periodicRunId, @periodicStepId, @message, @messageType, @logTime)",
                runIdParam, stepIdParam, messageParam, typeParam, logTimeParam);
        }

        /// <summary>
        /// Finishes a specific Run and sets its end-Time
        /// </summary>
        /// <param name="task"></param>
        /// <param name="runId"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        private bool FinishRun(PeriodicTask task, int runId, IDbWrapper database)
        {
            bool retVal;
            var taskRun = task.GetTemporaryValue<int>("CurrentRunId");
            retVal = taskRun == runId;
            if (retVal)
            {
                database.ExecuteCommand("update PeriodicRuns set endTime = @endTime where periodicRunId = @runId",
                    database.GetParameter("endTime", DateTime.Now),
                    database.GetParameter("runId", runId));
            }

            return retVal;
        }
    }
}
