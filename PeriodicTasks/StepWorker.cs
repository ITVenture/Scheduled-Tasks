using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ITVComponents.Logging;
using ITVComponents.Plugins;
using ITVComponents.Plugins.SelfRegistration;
using ITVComponents.Settings;
using ITVComponents.Threading;
using ITVComponents.TypeConversion;
using PeriodicTasks.Remote;

namespace PeriodicTasks
{
    /// <summary>
    /// Defines a basic StepWorker instance
    /// </summary>
    public abstract class StepWorker:IConfigurablePlugin
    {
        /// <summary>
        /// Holds a list of previously initialized workers
        /// </summary>
        private static Dictionary<string, StepWorker> workers =
            new Dictionary<string, StepWorker>(StringComparer.OrdinalIgnoreCase);

        private int currentRuns = 0;

        private object runCounterLock;

        private ManualResetEvent runningLock;

        private ManualResetEvent canRunLock;

        /// <summary>
        /// Initializes a new instance of the StepWorker class
        /// </summary>
        protected StepWorker()
        {
            runningLock = new ManualResetEvent(true);
            canRunLock = new ManualResetEvent(true);
            runCounterLock = new object();
        }

        /// <summary>
        /// Gets or sets the UniqueName of this Plugin
        /// </summary>
        public string UniqueName { get; set; }

        /// <summary>
        /// Indicates whether this deferrable init-object is already initialized
        /// </summary>
        public bool Initialized { get; private set; }

        /// <summary>
        /// Gets a list of consumed sections by the implementing component
        /// </summary>
        public JsonSectionDefinition[] ConsumedSections { get; protected set; } = new JsonSectionDefinition[0];

        /// <summary>
        /// Gets a value indicating whether the component is currently in the configuration mode
        /// </summary>
        public bool Configuring { get; private set; }

        /// <summary>
        /// Indicates whether this Object requires immediate Initialization right after calling the constructor
        /// </summary>
        public bool ForceImmediateInitialization => false;

        /// <summary>
        /// Gets a value indicating whether this worker supports ansynchronous processing
        /// </summary>
        public virtual bool SupportAsync => false;

        /// <summary>
        /// Instructs the Plugin to read the JsonSettings or to create a default instance if none is available
        /// </summary>
        public virtual void ReadSettings()
        {
        }

        /// <summary>Initializes this deferred initializable object</summary>
        public void Initialize()
        {
            if (!Initialized)
            {
                try
                {
                    lock (workers)
                    {

                        workers.Add(UniqueName, this);
                    }

                    Init();
                }
                finally
                {
                    Initialized = true;
                }
            }
        }

        /// <summary>
        /// Gets the worker with the given name
        /// </summary>
        /// <param name="uniqueName">the unique name of the desired StepWorker</param>
        /// <returns>the instance of the requested worker</returns>
        public static StepWorker GetWorker(string uniqueName)
        {
            lock (workers)
            {
                return workers.ContainsKey(uniqueName) ? workers[uniqueName] : null;
            }
        }

        /// <summary>
        /// Gets all initialized worker names
        /// </summary>
        /// <returns>an array containing all initialized workers</returns>
        public static string[] GetInitializedWorkers()
        {
            lock (workers)
            {
                return workers.Keys.ToArray();
            }
        }

        /// <summary>
        /// Runs a step of a given task
        /// </summary>
        /// <param name="task">the task that owns the current step</param>
        /// <param name="command">the command that will be evaluated by this worker</param>
        /// <param name="values">the variables that hold results of previous steps</param>
        /// <returns>the result of the provided command</returns>
        public async Task<object> Run(PeriodicTask task, string command, Dictionary<StepParameter, object> values)
        {
            bool ok = RegisterRun();
            if (ok)
            {
                try
                {
                    if (SupportAsync)
                    {
                        return await RunTaskAsync(task, command, values);
                    }
                    else
                    {
                        return RunTask(task, command, values);
                    }
                }
                finally
                {
                    CleanupRun();
                }
            }

            task.Fail("This StepWorker is currently in Configuration mode and can therefore not process any tasks.");
            return null;
        }

        /// <summary>
        /// Führt anwendungsspezifische Aufgaben durch, die mit der Freigabe, der Zurückgabe oder dem Zurücksetzen von nicht verwalteten Ressourcen zusammenhängen.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public virtual void Dispose()
        {
            OnDisposed();
        }

        /// <summary>
        /// Describes this worker
        /// </summary>
        /// <returns>a descriptor for this worker</returns>
        public virtual WorkerDescription Describe()
        {
            WorkerDescription retVal = new WorkerDescription {Type = GetType().Name, ExpectedParameters= new ParameterDescription[0], Remarks=string.Empty};
            return retVal;
        }

        /// <summary>
        /// Suspends all tasks executed by this component and waits for new settings
        /// </summary>
        public void EnterConfigurationMode()
        {
            canRunLock.Reset();
            runningLock.WaitOne();
            Configuring = true;
        }

        /// <summary>
        /// Resumes all tasks, after the new configuration settings have been applied
        /// </summary>
        public void LeaveConfigurationMode()
        {
            try
            {
                ReloadConfig();
            }
            finally
            {
                Configuring = false;
                canRunLock.Set();
            }
        }

        /// <summary>
        /// Raises the Disposed event
        /// </summary>
        protected virtual void OnDisposed()
        {
            EventHandler handler = Disposed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Runs the given Task synchronous
        /// </summary>
        /// <param name="task">the task to run</param>
        /// <param name="command">the provided command for the task</param>
        /// <param name="values">the values that were generated by previous steps</param>
        /// <returns>an object that contains the result of this method</returns>
        protected abstract object RunTask(PeriodicTask task, string command, Dictionary<StepParameter, object> values);

        /// <summary>
        /// Runs the given Task asynchronous
        /// </summary>
        /// <param name="task">the task to run</param>
        /// <param name="command">the provided command for the task</param>
        /// <param name="values">the values that were generated by previous steps</param>
        /// <returns>an awaitable object that contains the result of this method</returns>
        protected virtual Task<object> RunTaskAsync(PeriodicTask task, string command, Dictionary<StepParameter, object> values)
        {
            var retVal = RunTask(task, command, values);
            return Task.FromResult(retVal);
        }

        /// <summary>
        /// Runs Initializations on derived objects
        /// </summary>
        protected virtual void Init()
        {
        }

        /// <summary>
        /// Reloads the configuration for this worker
        /// </summary>
        protected virtual void ReloadConfig()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameterName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected T GetParameterValue<T>(string parameterName, Dictionary<StepParameter, object> parameters, PeriodicTask task)
        {
            T retVal = default(T);
            object objRaw = (from t in parameters where t.Key.ParameterName == parameterName select t.Value).FirstOrDefault();
            if (objRaw != null)
            {
                try
                {
                    retVal = (T) objRaw;
                }
                catch (Exception ex)
                {
                    task.Log(ex.Message, LogMessageType.Warning);
                    LogEnvironment.LogEvent(ex.ToString(), LogSeverity.Error);
                }
            }

            return retVal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="parameters"></param>
        /// <param name="task"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected object GetParameterValue(string parameterName, Dictionary<StepParameter, object> parameters, PeriodicTask task, Type type)
        {
            object retVal = GetDefault(type);
            object objRaw = (from t in parameters where t.Key.ParameterName == parameterName select t.Value).FirstOrDefault();
            if (objRaw != null)
            {
                try
                {
                    retVal = TypeConverter.Convert(objRaw, type) ?? retVal;
                }
                catch (Exception ex)
                {
                    task.Log(ex.Message, LogMessageType.Warning);
                    LogEnvironment.LogEvent(ex.ToString(), LogSeverity.Error);
                }
            }

            return retVal;
        }

        /// <summary>
        /// Gets the default value of the provided type
        /// </summary>
        /// <param name="type">the type for which to get the default value</param>
        /// <returns>the default value of the provided type</returns>
        private object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        /// <summary>
        /// Increases the number of active runs and causes the configuration-thread to wait until the execution was finished
        /// </summary>
        /// <returns></returns>
        private bool RegisterRun()
        {
            bool retVal = canRunLock.WaitOne(2000);
            if (retVal)
            {
                runningLock.Reset();
                lock (runCounterLock)
                {
                    currentRuns++;
                }
            }

            return retVal;
        }

        /// <summary>
        /// Un-Registers a run from the list of active runs on this worker
        /// </summary>
        private void CleanupRun()
        {
            lock (runCounterLock)
            {
                currentRuns--;
                if (currentRuns == 0)
                {
                    runningLock.Set();
                }
            }
        }

        /// <summary>
        /// Informs a calling class of a Disposal of this Instance
        /// </summary>
        public event EventHandler Disposed;
    }
}
