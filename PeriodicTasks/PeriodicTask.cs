using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ITVComponents.Helpers;
using ITVComponents.InterProcessCommunication.Shared.Helpers;
using ITVComponents.Logging;
using ITVComponents.ParallelProcessing;
using ITVComponents.Threading;
using PeriodicTasks.Events;

namespace PeriodicTasks
{
    [Serializable]
    public class PeriodicTask : TaskBase
    {
        /// <summary>
        /// the environment that runs this task and ensures that it stays alive
        /// </summary>
        [NonSerialized] private PeriodicEnvironment environment;

        /// <summary>
        /// Enables calling objects to perform tasks exclusively
        /// </summary>
        private object exclusiveLock = new object();

        /// <summary>
        /// holds a list of schedules for this task
        /// </summary>
        private List<SchedulerPolicy> schedules = new List<SchedulerPolicy>();

        /// <summary>
        /// Saves extended Metadata of this job.
        /// </summary>
        private Dictionary<string, object> taskMetaData = new Dictionary<string, object>();

        /// <summary>
        /// Holds temporary data that is only used during the runtime of the task
        /// </summary>
        private Dictionary<string, object> temporaryData = new Dictionary<string, object>();

        /// <summary>
        /// holds the single-run parameters for this task
        /// </summary>
        private Dictionary<string, object> singleRunParams;

        /// <summary>
        /// a list of steps for this task
        /// </summary>
        private TaskStep[] steps;

        /// <summary>
        /// Indicates whether this job is currently executing unsafe code
        /// </summary>
        private bool isUnsafe = false;

        /// <summary>
        /// Gets configured schedules for this Task
        /// </summary>
        public override ICollection<SchedulerPolicy> Schedules
        {
            get { return schedules; }
        }

        /// <summary>
        /// holds the priority of this task
        /// </summary>
        private int priority;

        private Random singleRunRnd;

        public PeriodicTask()
        {
            singleRunRnd = new Random();
        }

        public PeriodicTask(SerializationInfo info, StreamingContext context) : this()
        {
            var policies = (SchedulerPolicy[])info.GetValue("SchedulerPolicies", typeof(SchedulerPolicy[]));
            var meta = (Dictionary<string, object>)info.GetValue("CustomMetaData", typeof(Dictionary<string, object>));
            schedules.AddRange(policies);
            foreach (var item in meta)
            {
                taskMetaData[item.Key] = item.Value;
            }

            steps = (TaskStep[])info.GetValue("Steps", typeof(TaskStep[]));
            priority = info.GetInt32("PT##Priority");
        }

        /// <summary>Gets the priority of this task</summary>
        public override int Priority
        {
            get => priority;
            protected set => priority = value;
        }

        /// <summary>
        /// Gets a value indicating whether this job is currently executing unsafe code
        /// </summary>
        public override bool ExecutingUnsafe => isUnsafe;

        /// <summary>
        /// Gets or sets a value indicating whether this task is a single-run job, which is being awaited by an external consumer.
        /// </summary>
        public bool SingleRun { get; set; }

        /// <summary>
        /// Gets or sets the Steps of this task
        /// </summary>
        public TaskStep[] Steps
        {
            get { return steps; }
            set { steps = value; }
        }

        /// <summary>
        /// Gets the environment that is running this task
        /// </summary>
        public PeriodicEnvironment Environment
        {
            get { return environment; }
            internal set { environment = value; }
        }

        /// <summary>
        /// Gets or sets the unique name of a periodic task
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of an exclusive Execution Area. If Set, at the beginning of the Task, a NamedLock object is created and disposed at the end of the Execution.
        /// </summary>
        public string ExclusiveAreaName { get; set; }

        /// <summary>
        /// Gets or sets the last result of this Task
        /// </summary>
        internal object LastResult { get; set; }

        /// <summary>
        /// Initializes a single-run for the given task
        /// </summary>
        /// <param name="target">the target-configuration for this task</param>
        public void InitSingleRun(Dictionary<string, object> target)
        {
            if (!SingleRun)
            {
                throw new InvalidOperationException("This can only be used for single-Run configured tasks");
            }

            if (singleRunParams != null)
            {
                foreach (var tmp in singleRunParams)
                {
                    target[tmp.Key] = tmp.Value;
                }
            }

            singleRunParams = target;
        }

        /// <summary>
        /// the serialized exception that is associated with an error that causes this item not to process
        /// </summary>
        /// <param name="ex">the thrown exception</param>
        public override void Fail(SerializedException ex)
        {
            if (ex != null)
            {
                PeriodicTasksEventInterceptor.InterceptTaskMessage(environment, this, LogMessageType.Error,
                                                                   ex.ToString(), false);
            }
            else
            {
                PeriodicTasksEventInterceptor.InterceptTaskMessage(environment, this, LogMessageType.Error,
                                                                   "Task failed for an unknown reason", false);
            }
            
            if (ex != null)
            {
                Error = ex;
                LogEnvironment.LogEvent(ex.ToString(), LogSeverity.Error);
            }

            base.Fail(ex);
        }

        /// <summary>
        /// Indicates on this Task that it is currently executing unsafe code
        /// </summary>
        /// <returns>a ResourceLock that resets the unsafe-flag when disposed</returns>
        public override IDisposable Unsafe()
        {
            if (isUnsafe)
            {
                throw new InvalidOperationException("Already executing unsafe code!");
            }


            isUnsafe= true;
            return new UnsafeLock(()=>isUnsafe = false);
        }

        /// <summary>
        /// Sets values to indicate the success of the task
        /// </summary>
        public void Succeeded()
        {
            Error = null;
            Success = true;
            Fulfill();
        }

        /// <summary>
        /// Takes this task back to its initial status
        /// </summary>
        public void Reset()
        {
            temporaryData.Clear();
            Error = null;
            Success = false;
            ResetTask();
        }

        /// <summary>
        /// Sets values to indicate the failure of this task
        /// </summary>
        /// <param name="errorMessage">the error message that caused this task to fail</param>
        public void Fail(string errorMessage)
        {
            PeriodicTasksEventInterceptor.InterceptTaskMessage(environment, this, LogMessageType.Error, errorMessage, false);
            Success = false;
            Fulfill();
        }

        /// <summary>
        /// Logs progress of the current execution
        /// </summary>
        /// <param name="message">the message that is being logged</param>
        /// <param name="type">indicates the message-type of the current message</param>
        public void Log(string message, LogMessageType type)
        {
            PeriodicTasksEventInterceptor.InterceptTaskMessage(environment, this, type, message, false);
        }

        /// <summary>
        /// Logs progress of the current execution
        /// </summary>
        /// <param name="message">the message that is being logged</param>
        /// <param name="type">indicates the message-type of the current message</param>
        public void DebugLog(string message, LogMessageType type)
        {
            PeriodicTasksEventInterceptor.InterceptTaskMessage(environment, this, type, message, true);
        }

        /// <summary>
        /// Creates Meta-Data - Informations that can be used to identify a specific Task
        /// </summary>
        /// <returns>a Dictionary that is uniquely identifying a specific Task</returns>
        public override Dictionary<string, object> BuildMetaData()
        {
            Dictionary<string, object> retVal = new Dictionary<string, object>();
            retVal.Add("NAME", $"{Name}{(SingleRun ? $"_ONCE_{DateTime.Now.Ticks}_{singleRunRnd.Next(10000)}" : "")}");
            foreach (KeyValuePair<string, object> val in taskMetaData)
            {
                retVal[val.Key] = val.Value;
            }

            return retVal;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        /// <filterpriority>2</filterpriority>
        public override void Dispose()
        {
        }

        /// <summary>
        /// Demands exclusive Access for this Task
        /// </summary>
        /// <returns>a resourcelock that will be released when the exclusive access is no longer required</returns>
        public override IResourceLock DemandExclusive()
        {
            return new ResourceLock(exclusiveLock);
        }

        /// <summary>
        /// Sets the priority of this item to a new value
        /// </summary>
        /// <param name="newPriority">the new priority of a specific item</param>
        /// <param name="schedules">a list of schedules for this task</param>
        /// <param name="description">the description that is configured for this task</param>
        /// <param name="exclusiveAreaName">an exclusive area name that is used to ensure an exclusive execution of this task</param>
        public void Configure(int newPriority, SchedulerPolicy[] schedules, string description, string exclusiveAreaName)
        {
            Priority = newPriority;
            if (schedules != null)
            {
                ConfigureSchedule(schedules);
            }

            Description = description;
            ExclusiveAreaName = exclusiveAreaName;
        }

        /// <summary>
        /// Configures schedules for this periodicTask
        /// </summary>
        /// <param name="schedules">the schedules for this task</param>
        public void ConfigureSchedule(SchedulerPolicy[] schedules)
        {
            lock (exclusiveLock)
            {
                this.schedules.Clear();
                this.schedules.AddRange(schedules);
            }
        }

        /// <summary>
        /// Indicates whether this job Task is a duplicate of an other job
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool IsDuplicateOf(ITask other)
        {
            PeriodicTask t = null;
            bool retVal = (other is PeriodicTask ot) && (t=ot) != null && t.Name == Name;
            if (!retVal && t != null)
            {
                if (taskMetaData.Count != 0)
                {
                    retVal = t.TestMetaData(taskMetaData);
                }
            }

            return retVal;
        }

        public void SetTemporaryValue(string name, object value)
        {
            temporaryData[name] = value;
        }

        public T GetTemporaryValue<T>(string name)
        {
            var retVal = default(T);
            if (temporaryData.ContainsKey(name) && temporaryData[name] is T m)
            {
                retVal = m;
            }

            return retVal;
        }

        protected override void CompleteObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("SchedulerPolicies", schedules.ToArray());
            info.AddValue("CustomMetaData", taskMetaData);
            info.AddValue("Steps", steps);
            info.AddValue("PT##Priority", priority);
        }

        /// <summary>
        /// Gets a specific metadata information
        /// </summary>
        /// <param name="key">the metadata key</param>
        /// <returns>the value of the requested metadata key</returns>
        public object ReadMetaInformation(string key)
        {
            object retVal = null;
            var k = key.ToUpper();
            if (taskMetaData.ContainsKey(k))
            {
                retVal = taskMetaData[k];
            }

            return retVal;
        }

        /// <summary>
        /// Clears the last final result of this task
        /// </summary>
        internal void ClearLastResult()
        {
            LastResult = null;
        }

        /// <summary>
        /// Tests the MetaData of a different job or a job that must be re-configured
        /// </summary>
        /// <param name="metaData">the meta-data for which to test this job</param>
        /// <returns>a value indicating whether the MetaData of this job is equal to the provided data</returns>
        internal bool TestMetaData(Dictionary<string, object> metaData)
        {
            bool retVal = metaData != null || taskMetaData.Count == 0;
            if (metaData != null)
            {
                if (retVal = (metaData.Count == taskMetaData.Count))
                {
                    foreach (string s in taskMetaData.Keys)
                    {
                        retVal &= (metaData.ContainsKey(s) &&
                                   ((metaData[s] == null && taskMetaData[s] == null) ||
                                    metaData[s].Equals(taskMetaData[s])));
                        if (!retVal)
                        {
                            break;
                        }
                    }
                }
            }

            return retVal;
        }

        /// <summary>
        /// Sets a Meta-Information value of this task. This information will also be used for detecting duplicate jobs
        /// </summary>
        /// <param name="metaData">the metaData that was delivered from the taskloader for this task</param>
        internal void SetMetaDataInformation(Dictionary<string,object> metaData)
        {
            foreach (KeyValuePair<string, object> value in metaData)
            {
                taskMetaData[value.Key.ToUpper()] = value.Value;
            }
        }
    }
}
