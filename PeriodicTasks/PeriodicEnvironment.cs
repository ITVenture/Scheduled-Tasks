using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ITVComponents.Logging;
using ITVComponents.ParallelProcessing;
using ITVComponents.Plugins;
using ITVComponents.Security;
using ITVComponents.Serialization;
using ITVComponents.Settings;
using ITVComponents.Threading;
using PeriodicTasks.Properties;
using PeriodicTasks.Remote;

namespace PeriodicTasks
{
    /// <summary>
    /// Provides an environment for task executions
    /// </summary>
    public class PeriodicEnvironment : IConfigurablePlugin
    {
        /// <summary>
        /// the parallelprocessor that executes all scheduled tasks
        /// </summary>
        private ParallelTaskProcessor<PeriodicTask> processor;

        /// <summary>
        /// The number of priorities used by the processor
        /// </summary>
        private readonly int priorityCount;

        /// <summary>
        /// the number of workers used by the processor
        /// </summary>
        private readonly int workerCount;

        /// <summary>
        /// the poll-timeout of the processor
        /// </summary>
        private readonly int workerPollTime;

        /// <summary>
        /// TaskLoader instance that is used for loading pending tasks from a supported source
        /// </summary>
        private ITaskLoader taskLoader;

        /// <summary>
        /// Holds a list of known tasks for the current environment
        /// </summary>
        private List<PeriodicTask> knownTasks = new List<PeriodicTask>();

        /// <summary>
        /// Initializes static members of the PeriodicEnvironment class
        /// </summary>
        static PeriodicEnvironment()
        {
            InitializeEncryption();
        }

        /// <summary>
        /// Initializes a new instance of the PeriodicEnvironment class
        /// </summary>
        /// <param name="priorityCount">the number of priorities of this periodic environment</param>
        /// <param name="workerCount">the number of parallel tasks</param>
        /// <param name="workerPollTime">the PollTime of the parallel Queue</param>
        /// <param name="taskLoader">a taskLoader strategy that is used to access tasks that are defined in a supported source</param>
        public PeriodicEnvironment(int priorityCount, int workerCount, int workerPollTime, ITaskLoader taskLoader)
            : this(priorityCount,workerCount,workerPollTime,taskLoader, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the PeriodicEnvironment class
        /// </summary>
        /// <param name="priorityCount">the number of priorities of this periodic environment</param>
        /// <param name="workerCount">the number of parallel tasks</param>
        /// <param name="workerPollTime">the PollTime of the parallel Queue</param>
        /// <param name="taskLoader">a taskLoader strategy that is used to access tasks that are defined in a supported source</param>
        public PeriodicEnvironment(int priorityCount, int workerCount, int workerPollTime, ITaskLoader taskLoader, bool enableDebugMessages)
            : this()
        {
            this.priorityCount = priorityCount;
            this.workerCount = workerCount;
            this.workerPollTime = workerPollTime;
            this.taskLoader = taskLoader;
            this.DebugEnabled = enableDebugMessages;
        }

        /// <summary>
        /// Gets a list of consumed sections by the implementing component
        /// </summary>
        public JsonSectionDefinition[] ConsumedSections { get; } = new JsonSectionDefinition[0];

        /// <summary>
        /// Gets a value indicating whether the component is currently in the configuration mode
        /// </summary>
        public bool Configuring { get; private set; }

        /// <summary>
        /// Indicates whether this deferrable init-object is already initialized
        /// </summary>
        public bool Initialized { get; private set; }

        /// <summary>
        /// Indicates whether debug messages are used by this environment istance
        /// </summary>
        internal bool DebugEnabled { get; private set; }

        /// <summary>
        /// Initializes the encryption for the PeriodicTask environment
        /// </summary>
        public static void InitializeEncryption()
        {
            PasswordSecurity.InitializeAes(Resources.TasksEntropy);
        }

        public void EnterConfigurationMode()
        {
            if (Initialized)
            {
                processor.Suspend();
            }
        }

        public void LeaveConfigurationMode()
        {
            if (Initialized)
            {
                processor.Resume();
            }
        }

        public void Initialize()
        {
            if (!Initialized)
            {
                processor = new ParallelTaskProcessor<PeriodicTask>(this.UniqueName + "TaskProcessor", () => new PeriodicTaskProcessor(this), 1, priorityCount,
                    workerCount, workerPollTime, workerCount - 1,
                    workerCount + 2, false, false, true);
                processor.GetMoreTasks += RefreshTasks;
                processor.Initialize();
                Initialized = true;
            }
        }
        public bool ForceImmediateInitialization { get; }
        public void ReadSettings()
        {
        }

        /// <summary>
        /// Gets a list of Initialized workers
        /// </summary>
        /// <returns>a list of all names of initialized workers</returns>
        public string[] GetStepWorkers()
        {
            return StepWorker.GetInitializedWorkers();
        }

        public WorkerDescription DescribeWorker(string workerName)
        {
            WorkerDescription retVal = null;
            StepWorker worker = StepWorker.GetWorker(workerName);
            if (worker != null)
            {
                retVal = worker.Describe();
            }

            return retVal;
        }

        public async Task<object> InvokeWithParams(string taskName, Dictionary<string, object> arguments)
        {
            var t = taskLoader.GetRunOnceTask(taskName, (s, m) =>
            {
                var tmp = new PeriodicTask { Name = s };
                if (m != null)
                {
                    tmp.SetMetaDataInformation(m);
                }

                return tmp;
            }, t => { });

            t.Environment = this;
            t.InitSingleRun(arguments);
            var d = await processor.ProcessAsync(t);
            Console.WriteLine(d.LastResult);
            return d.LastResult;
        }

        /// <summary>
        /// Refreshes the tasks that are processed in this Environment
        /// </summary>
        /// <param name="sender">the event-sender</param>
        /// <param name="e">the event arguments</param>
        private void RefreshTasks(object sender, GetMoreTasksEventArgs e)
        {
            List<PeriodicTask> newTasks = new List<PeriodicTask>();
            int newTaskCounter = 0;
            taskLoader.RefreshTasks(e.Priority, (n, m) =>
            {
                var tmp =
                    (from t in knownTasks
                        where
                            (t.Name == n && t.TestMetaData(m)) ||
                            (m != null && m.Count != 0 && t.TestMetaData(m))
                        select t)
                    .FirstOrDefault();
                if (tmp == null)
                {
                    newTaskCounter++;
                    tmp = new PeriodicTask() { Name = n };
                    if (m != null)
                    {
                        tmp.SetMetaDataInformation(m);
                    }

                    lock (knownTasks)
                    {
                        knownTasks.Add(tmp);
                    }
                }

                using (var lk = tmp.DemandExclusive())
                {
                    lk.Exclusive(() =>
                    {
                        if (tmp.Running || tmp.Configuring)
                        {
                            tmp.WaitExclusive();
                        }

                        tmp.Configuring = true;
                    });
                }


                if (!tmp.Active)
                {
                    newTasks.Add(tmp);
                }

                return tmp;
            }, t =>
            {
                using (var lk = t.DemandExclusive())
                {
                    lk.Exclusive(() =>
                    {
                        t.Configuring = false;
                        t.PulseExclusive();

                    });
                }
            });
            LogEnvironment.LogDebugEvent($"Found {newTaskCounter} new Tasks ({newTasks.Count} in newTasks-list).", LogSeverity.Report);
            for (int index = 0; index < newTasks.Count; index++)
            {
                PeriodicTask t = newTasks[index];
                if (t.Active)
                {
                    t.Environment = this;
                    var ok = processor.EnqueueTask(t);
                    if (!ok)
                    {
                        LogEnvironment.LogEvent($"Task {t.Name} could not be enqueued.", LogSeverity.Warning);
                        t.Active = false;
                    }
                }
            }
        }

        /// <summary>
        /// Prevents a default instance of the PeriodicEnvironment Class from being created
        /// </summary>
        private PeriodicEnvironment()
        {
        }

        /// <summary>
        /// Gets or sets the UniqueName of this Plugin
        /// </summary>
        public string UniqueName { get; set; }

        /// <summary>
        /// Führt anwendungsspezifische Aufgaben durch, die mit der Freigabe, der Zurückgabe oder dem Zurücksetzen von nicht verwalteten Ressourcen zusammenhängen.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (Initialized)
            {
                processor.Stop();
                processor.Dispose();
            }

            OnDisposed();
        }

        /// <summary>
        /// Requeues the current task for execution
        /// </summary>
        /// <param name="periodicTask">the periodic task that requires execution</param>
        public bool Requeue(PeriodicTask periodicTask)
        {
            periodicTask.Reset();
            return processor.EnqueueTask(periodicTask);
        }

        /// <summary>
        /// Stops Processes that are running inside the current Plugin
        /// </summary>
        public void Stop()
        {
            if (Initialized)
            {
                processor.Stop();
            }
        }

        /// <summary>
        /// Raises the disposed event
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

        /// <summary>
        /// Removes a task from the list of known tasks or attempts to force a re-activation of it by explicitly setting it to inactive.
        /// </summary>
        /// <param name="task">a Task tha could not properly be re-queued by the task-worker</param>
        public void UnRegisterTask(PeriodicTask task)
        {
            bool dispose = true;
            try
            {
                lock (knownTasks)
                {
                    var t = knownTasks.FirstOrDefault(n => n == task || n.TestMetaData(task.BuildMetaData()));
                    bool ok = false;
                    if (t != null)
                    {
                        task.Log("Removing Task from list, so it can be re-fetched on next task-refresh.", LogMessageType.Report);
                        ok = knownTasks.Remove(t);
                    }

                    if (!ok)
                    {
                        task.Log("Task was not found. Setting it to Inactive..", LogMessageType.Warning);
                        dispose = false;
                        task.Active = false;
                    }
                }
            }
            catch (Exception ex)
            {
                task.Log($"Failed to unregister task: {ex.Message}. Setting it to inactive", LogMessageType.Error);
                dispose = false;
                task.Active = false;
            }
            finally
            {
                if (dispose)
                {
                    task.Dispose();
                }
            }
        }
    }
}