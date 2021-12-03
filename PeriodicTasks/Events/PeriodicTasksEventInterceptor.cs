using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ITVComponents.DataAccess.Extensions;
using ITVComponents.Logging;
using ITVComponents.Plugins;

namespace PeriodicTasks.Events
{
    /// <summary>
    /// Intercepts Task events
    /// </summary>
    public abstract class  PeriodicTasksEventInterceptor:IPlugin
    {
        /// <summary>
        /// Holds interceptors for the current app-domain
        /// </summary>
        private static readonly List<PeriodicTasksEventInterceptor> interceptors;

        /// <summary>
        /// indicates whether this instance has been disposed
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Initializes static members of the PeriodicTasksEventInterceptor class
        /// </summary>
        static PeriodicTasksEventInterceptor()
        {
            interceptors = new List<PeriodicTasksEventInterceptor>();
        }

        /// <summary>
        /// Initializes a new instance of the PeriodicTaskEventInterceptor class
        /// </summary>
        /// <param name="environment">the task environment for which to register this interceptor</param>
        protected PeriodicTasksEventInterceptor(PeriodicEnvironment environment)
        {
            Environment = environment;
            lock (interceptors)
            {
                interceptors.Add(this);
            }
        }

        /// <summary>
        /// Gets or sets the UniqueName of this Plugin
        /// </summary>
        public string UniqueName { get; set; }

        /// <summary>
        /// Gets the Environment for which to intercept task events
        /// </summary>
        protected PeriodicEnvironment Environment { get; private set; }

        /// <summary>
        /// Führt anwendungsspezifische Aufgaben durch, die mit der Freigabe, der Zurückgabe oder dem Zurücksetzen von nicht verwalteten Ressourcen zusammenhängen.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public virtual void Dispose()
        {
            if (!disposed)
            {
                try
                {
                    lock (interceptors)
                    {
                        interceptors.Remove(this);
                    }

                    OnDisposed();
                }
                finally
                {
                    disposed = true;
                }
            }
        }

        /// <summary>
        /// Intercepts the Task-Started event on the provided task
        /// </summary>
        /// <param name="environment">the periodic environment running the task</param>
        /// <param name="target">the target task that has started</param>
        /// <param name="variables">the variables of the given task</param>
        internal static void InterceptTaskStarts(PeriodicEnvironment environment, PeriodicTask target,
                                                 Dictionary<string, object> variables)
        {
            try
            {
                lock (interceptors)
                {
                    (from t in interceptors where t.Environment == environment select t).ForEach(
                        n => n.InterceptTaskStarts(target, variables));
                }
            }
            catch (Exception ex)
            {
                LogEnvironment.LogEvent(ex.ToString(), LogSeverity.Error);
            }
        }

        /// <summary>
        /// Intercepts the Task-Ended event on the provided task
        /// </summary>
        /// <param name="environment">the periodic environment running the task</param>
        /// <param name="target">the target task that has ended</param>
        /// <param name="variables">the variables of the given task</param>
        internal static void InterceptTaskEnds(PeriodicEnvironment environment, PeriodicTask target,
                                               Dictionary<string, object> variables)
        {
            try
            {
                lock (interceptors)
                {
                    (from t in interceptors where t.Environment == environment select t).ForEach(
                        n => n.InterceptTaskEnds(target, variables));
                }
            }
            catch (Exception ex)
            {
                LogEnvironment.LogEvent(ex.ToString(), LogSeverity.Error);
            }
        }

        /// <summary>
        /// Intercepts the Task-Ended event on the provided task
        /// </summary>
        /// <param name="environment">the periodic environment running the task</param>
        /// <param name="target">the target task that has ended</param>
        /// <param name="variables">the variables of the given task</param>
        internal static void InterceptTaskEndsWithError(PeriodicEnvironment environment, PeriodicTask target,
            Dictionary<string, object> variables)
        {
            try
            {
                lock (interceptors)
                {
                    (from t in interceptors where t.Environment == environment select t).ForEach(
                        n => n.InterceptTaskEndsWithError(target, variables));
                }
            }
            catch (Exception ex)
            {
                LogEnvironment.LogEvent(ex.ToString(), LogSeverity.Error);
            }
        }

        /// <summary>
        /// Intercepts the event before-step on the provided task
        /// </summary>
        /// <param name="environment">the periodic environment running the task</param>
        /// <param name="target">the task on which a step is about to be executed</param>
        /// <param name="step">the step that is executing</param>
        /// <param name="stepId">the sequence-identity (1..n) of the current step</param>
        /// <param name="stepCount">the total number of steps on the current task</param>
        /// <param name="variables">the variables for the given job</param>
        internal static void InterceptBeforeTaskStep(PeriodicEnvironment environment, PeriodicTask target, TaskStep step,int stepId, int stepCount,
                                                     Dictionary<string, object> variables)
        {
            try
            {
                lock (interceptors)
                {
                    (from t in interceptors where t.Environment == environment select t).ForEach(
                        n => n.InterceptBeforeTaskStep(target, step, stepId, stepCount, variables));
                }
            }
            catch (Exception ex)
            {
                LogEnvironment.LogEvent(ex.ToString(), LogSeverity.Error);
            }
        }

        /// <summary>
        /// Intercepts the event after-step on the provided task
        /// </summary>
        /// <param name="environment">the periodic environment running the task</param>
        /// <param name="target">the task that has finished a specific step</param>
        /// <param name="step">the step that has finished execution</param>
        /// <param name="result">the result that was returned by the worker of the given step</param>
        /// <param name="stepId">the sequence-identity (1..n) of the current step</param>
        /// <param name="stepCount">the total number of steps on the current task</param>
        /// <param name="variables">the variables of the given job</param>
        internal static void InterceptAfterTaskStep(PeriodicEnvironment environment, PeriodicTask target, TaskStep step, int stepId, int stepCount, object result,
                                                    Dictionary<string, object> variables)
        {
            try
            {
                lock (interceptors)
                {
                    (from t in interceptors where t.Environment == environment select t).ForEach(
                        n => n.InterceptAfterTaskStep(target, step, stepId, stepCount, result, variables));
                }
            }
            catch (Exception ex)
            {
                LogEnvironment.LogEvent(ex.ToString(), LogSeverity.Error);
            }
        }

        /// <summary>
        /// Intercepts the event when a job tries to log a message
        /// </summary>
        /// <param name="environment">the periodic environment running the task</param>
        /// <param name="target">the target job that has generated a message</param>
        /// <param name="messageType">the type of the message</param>
        /// <param name="message">the message that was generated</param>
        /// <param name="debug">indicates whether the message is for debug purposes only</param>
        internal static void InterceptTaskMessage(PeriodicEnvironment environment, PeriodicTask target,
                                           LogMessageType messageType, string message, bool debug)
        {
            if (!debug || environment.DebugEnabled)
            {
                try
                {
                    lock (interceptors)
                    {
                        (from t in interceptors where t.Environment == environment select t).ForEach(
                            n =>
                            {
                                n.InterceptTaskMessage(target, messageType, message);
                            });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    LogEnvironment.LogEvent(ex.ToString(), LogSeverity.Error);
                }
            }
        }

        /// <summary>
        /// Intercepts the event when a job terminates because of a condition that did not apply
        /// </summary>
        /// <param name="environment">the periodic environment running the task</param>
        /// <param name="target">the target job that has generated a message</param>
        internal static void InterceptTaskTerminatesPlanned(PeriodicEnvironment environment, PeriodicTask target)
        {
            try
            {
                lock (interceptors)
                {
                    (from t in interceptors where t.Environment == environment select t).ForEach(
                        n => n.InterceptTaskTerminatesPlanned(target));
                }
            }
            catch (Exception ex)
            {
                LogEnvironment.LogEvent(ex.ToString(), LogSeverity.Error);
            }
        }

        /// <summary>
        /// Intercepts the event when a job terminates because of the RunCondition of a Step that resulted to false
        /// </summary>
        /// <param name="environment">the periodic environment running the task</param>
        /// <param name="target">the target job that has generated a message</param>
        /// <param name="step">the step that has caused to termination of a Task execution</param>
        /// <param name="stepId">the sequence-identity (1..n) of the current step</param>
        /// <param name="stepCount">the total number of steps on the current task</param>
        internal static void InterceptTaskTerminationDueToRunCondition(PeriodicEnvironment environment, PeriodicTask target, TaskStep step, int stepId, int stepCount)
        {
            try
            {
                lock (interceptors)
                {
                    (from t in interceptors where t.Environment == environment select t).ForEach(
                        n => n.InterceptTaskTerminationDueToRunCondition(target, step, stepId, stepCount));
                }
            }
            catch (Exception ex)
            {
                LogEnvironment.LogEvent(ex.ToString(), LogSeverity.Error);
            }
        }

        /// <summary>
        /// Intercepts the Task-Started event on the provided task
        /// </summary>
        /// <param name="target">the target task that has started</param>
        /// <param name="variables">the variables of the given task</param>
        protected abstract void InterceptTaskStarts(PeriodicTask target, Dictionary<string, object> variables);

        /// <summary>
        /// Intercepts the Task-Ended event on the provided task
        /// </summary>
        /// <param name="target">the target task that has ended</param>
        /// <param name="variables">the variables of the given task</param>
        protected abstract void InterceptTaskEnds(PeriodicTask target, Dictionary<string, object> variables);

        /// <summary>
        /// Intercepts the Task-Ended event on the provided task
        /// </summary>
        /// <param name="target">the target task that has ended</param>
        /// <param name="variables">the variables of the given task</param>
        protected abstract void InterceptTaskEndsWithError(PeriodicTask target, Dictionary<string, object> variables);

        /// <summary>
        /// Intercepts the event before-step on the provided task
        /// </summary>
        /// <param name="target">the task on which a step is about to be executed</param>
        /// <param name="step">the step that is executing</param>
        /// <param name="stepId">the sequence-identity (1..n) of the current step</param>
        /// <param name="stepCount">the total number of steps on the current task</param>
        /// <param name="variables">the variables for the given job</param>
        protected abstract void InterceptBeforeTaskStep(PeriodicTask target, TaskStep step, int stepId, int stepCount, Dictionary<string, object> variables);

        /// <summary>
        /// Intercepts the event after-step on the provided task
        /// </summary>
        /// <param name="target">the task that has finished a specific step</param>
        /// <param name="step">the step that has finished execution</param>
        /// <param name="stepId">the sequence-identity (1..n) of the current step</param>
        /// <param name="stepCount">the total number of steps on the current task</param>
        /// <param name="result">the result that was returned by the worker of the given step</param>
        /// <param name="variables">the variables of the given job</param>
        protected abstract void InterceptAfterTaskStep(PeriodicTask target, TaskStep step, int stepId, int stepCount, object result, Dictionary<string, object> variables);

        /// <summary>
        /// Intercepts the event when a job tries to log a message
        /// </summary>
        /// <param name="target">the target job that has generated a message</param>
        /// <param name="messageType">the type of the message</param>
        /// <param name="message">the message that was generated</param>
        protected abstract void InterceptTaskMessage(PeriodicTask target, LogMessageType messageType, string message);

        /// <summary>
        /// Intercepts the event when a job terminates because of a condition that did not apply
        /// </summary>
        /// <param name="target">the target job that has generated a message</param>
        protected abstract void InterceptTaskTerminatesPlanned(PeriodicTask target);

        /// <summary>
        /// Intercepts the event when a job terminates because of the RunCondition of a Step that resulted to false
        /// </summary>
        /// <param name="target">the target job that has generated a message</param>
        /// <param name="step">the step that has caused to termination of a Task execution</param>
        /// <param name="stepId">the sequence-identity (1..n) of the current step</param>
        /// <param name="stepCount">the total number of steps on the current task</param>
        protected abstract void InterceptTaskTerminationDueToRunCondition(PeriodicTask target, TaskStep step, int stepId, int stepCount);

        /// <summary>
        /// Raises the Disposed event
        /// </summary>
        protected virtual void OnDisposed()
        {
            EventHandler handler = Disposed;
            if (handler != null) handler(this, EventArgs.Empty);
        }


        /// <summary>
        /// Informs a calling class of a Disposal of this Instance
        /// </summary>
        public event EventHandler Disposed;
    }
}
