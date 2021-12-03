using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ITVComponents.Plugins;
using ITVComponents.Settings;
using ITVComponents.Threading;
using PeriodicTasks.AppConfigBasedTaskLoading.Config;

namespace PeriodicTasks.AppConfigBasedTaskLoading
{
    /// <summary>
    /// Provides AppConfig based configuration capabilities for TaskScheduling
    /// </summary>
    public class AppConfigBasedLoader:ITaskLoader, IDeferredInit, IConfigurablePlugin
    {
        /// <summary>
        /// Prevents the concurrent read/write access on the Configuring property
        /// </summary>
        private object configuringLock = new object();

        /// <summary>
        /// indicates whether the configuration is currently being read
        /// </summary>
        private bool configReading = false;

        /// <summary>
        /// Gets or sets the UniqueName of this Plugin
        /// </summary>
        public string UniqueName { get; set; }

        /// <summary>
        /// Indicates whether this deferrable init-object is already initialized
        /// </summary>
        public bool Initialized { get; }

        /// <summary>
        /// Indicates whether this Object requires immediate Initialization right after calling the constructor
        /// </summary>
        public bool ForceImmediateInitialization { get; }

        /// <summary>
        /// Gets a list of consumed sections by the implementing component
        /// </summary>
        public JsonSectionDefinition[] ConsumedSections { get; } = new[] {new JsonSectionDefinition {Name = "PeriodicTasks_AppConfigBasedTaskLoading_TaskConfiguration", SettingsType = typeof(TaskConfig)}};

        /// <summary>
        /// Gets a value indicating whether the component is currently in the configuration mode
        /// </summary>
        public bool Configuring { get; private set; }

        /// <summary>Initializes this deferred initializable object</summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Instructs the Plugin to read the JsonSettings or to create a default instance if none is available
        /// </summary>
        public void ReadSettings()
        {
            var tmp = TaskConfig.Helper.Tasks;
        }

        /// <summary>
        /// Suspends all tasks executed by this component and waits for new settings
        /// </summary>
        public void EnterConfigurationMode()
        {
            lock (configuringLock)
            {
                while (configReading)
                {
                    Monitor.Wait(configuringLock, 500);
                }

                Configuring = true;
            }
        }

        /// <summary>
        /// Resumes all tasks, after the new configuration settings have been applied
        /// </summary>
        public void LeaveConfigurationMode()
        {
            lock (configuringLock)
            {
                Configuring = false;
                Monitor.PulseAll(configuringLock);
            }
        }

        /// <summary>
        /// Refreshes Tasks that are stored in a source that can be accessed with the implemented strategy
        /// </summary>
        /// <param name="priority">the priority for which to load tasks</param>
        /// <param name="getTask">callback for getting the raw-task for a specific name, so it can be refreshed</param>
        public void RefreshTasks(int priority, Func<string, Dictionary<string,object>, PeriodicTask> getTask)
        {
            lock (configuringLock)
            {
                while (Configuring)
                {
                    Monitor.Wait(configuringLock, 500);
                }

                configReading = true;
            }

            try
            {
                TaskCollection tasks;
                lock (TaskConfig.SettingsLock)
                {
                    bool success = false;
                    while (!success)
                    {
                        try
                        {
                            TaskConfig.Helper.Reload();
                            success = true;
                        }
                        catch (Exception)
                        {
                            success = false;
                        }
                    }

                    tasks = TaskConfig.Helper.Tasks;
                }

                foreach (TaskDefinition def in tasks.Where(t => t.Priority == priority))
                {
                    PeriodicTask target = getTask(def.TaskName, null);
                    target.Active = def.Active;
                    target.Configure(def.Priority, def.Schedules.ToArray(), def.Description, def.ExclusiveAreaName);
                    target.Steps = (from t in def.Steps
                        orderby t.Order
                        select new TaskStep
                        {
                            Command = t.Command,
                            Order = t.Order,
                            OutputVariable = t.OutputVariable,
                            StepName = t.Name,
                            StepWorkerName = t.WorkerName,
                            RunCondition = t.RunCondition,
                            ExclusiveAreaName = t.ExclusiveAreaName,
                            Parameters = (from u in t.Parameters
                                select
                                    new StepParameter
                                    {
                                        ParameterName = u.ParameterName,
                                        ParameterSettings = u.ParameterSettings,
                                        ParameterType = u.ParameterType,
                                        Value = u.Value
                                    }).ToArray()
                        }).ToArray();
                }
            }
            finally
            {
                lock (configuringLock)
                {
                    configReading = false;
                    Monitor.PulseAll(configuringLock);
                }
            }
        }

        public PeriodicTask GetRunOnceTask(string name, Func<string, Dictionary<string, object>, PeriodicTask> getTask)
        {
            lock (configuringLock)
            {
                while (Configuring)
                {
                    Monitor.Wait(configuringLock, 500);
                }

                configReading = true;
            }

            try
            {
                TaskCollection tasks;
                lock (TaskConfig.SettingsLock)
                {
                    bool success = false;
                    while (!success)
                    {
                        try
                        {
                            TaskConfig.Helper.Reload();
                            success = true;
                        }
                        catch (Exception)
                        {
                            success = false;
                        }
                    }

                    tasks = TaskConfig.Helper.Tasks;
                }

                TaskDefinition def = tasks.FirstOrDefault(t => t.TaskName == name);
                if(def != null)
                {
                    PeriodicTask target = getTask(def.TaskName, null);
                    target.Active = true;
                    target.SingleRun = true;
                    target.Configure(def.Priority, null, def.Description, def.ExclusiveAreaName);
                    target.Steps = (from t in def.Steps
                                    orderby t.Order
                                    select new TaskStep
                                    {
                                        Command = t.Command,
                                        Order = t.Order,
                                        OutputVariable = t.OutputVariable,
                                        StepName = t.Name,
                                        StepWorkerName = t.WorkerName,
                                        RunCondition = t.RunCondition,
                                        ExclusiveAreaName = t.ExclusiveAreaName,
                                        Parameters = (from u in t.Parameters
                                                      select
                                                          new StepParameter
                                                          {
                                                              ParameterName = u.ParameterName,
                                                              ParameterSettings = u.ParameterSettings,
                                                              ParameterType = u.ParameterType,
                                                              Value = u.Value
                                                          }).ToArray()
                                    }).ToArray();

                    return target;
                }
            }
            finally
            {
                lock (configuringLock)
                {
                    configReading = false;
                    Monitor.PulseAll(configuringLock);
                }
            }

            return null;
        }

        /// <summary>
        /// Führt anwendungsspezifische Aufgaben durch, die mit der Freigabe, der Zurückgabe oder dem Zurücksetzen von nicht verwalteten Ressourcen zusammenhängen.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            OnDisposed();
        }

        /// <summary>
        /// Raises the Disposed event
        /// </summary>
        protected virtual void OnDisposed()
        {
            Disposed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Informs a calling class of a Disposal of this Instance
        /// </summary>
        public event EventHandler Disposed;
    }
}
