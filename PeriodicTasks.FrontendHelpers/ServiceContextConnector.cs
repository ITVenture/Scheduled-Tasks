using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dynamitey.DynamicObjects;
using ITVComponents.Formatting;
using ITVComponents.InterProcessCommunication.Shared.Base;
using ITVComponents.InterProcessCommunication.ManagementExtensions.Scheduling;
using ITVComponents.Plugins;
using ITVComponents.Plugins.PluginServices;
using ITVComponents.WebCoreToolkit.WebPlugins.InjectablePlugins;
using PeriodicTasks.FrontendHelpers.Models;
using PeriodicTasks.Remote;

namespace PeriodicTasks.FrontendHelpers
{
    [ScopedDependency(FriendlyName = "TaskEnvironment")]
    public class ServiceContextConnector:IPlugin
    {
        /// <summary>
        /// The remote client that is used to communicate with the scheduling-manager - object
        /// </summary>
        private IBaseClient client;

        /// <summary>
        /// The Scheduling - Manager for this serviceContext-connector
        /// </summary>
        private ISchedulingManager manager;

        /// <summary>
        /// the Environment object that provides further information about the current service environment
        /// </summary>
        private ITaskEnvironment remoteEnvironment;

        /// <summary>
        /// indicates whether the manager is ready
        /// </summary>
        private ManualResetEvent ready;

        /// <summary>
        /// Initializes a new instance of the ServiceContextConnector class
        /// </summary>
        public ServiceContextConnector(IBaseClient serviceClient, ISchedulingManager schedulerManager, string environmentName)
        {
            ready = new ManualResetEvent(false);
            client = serviceClient;
            manager = schedulerManager;
            remoteEnvironment = client.CreateProxy<ITaskEnvironment>(environmentName);
            client.OperationalChanged += (sender, args) => ready.Set();
        }

        /// <summary>
        /// Gets or sets the UniqueName of this Plugin
        /// </summary>
        public string UniqueName { get; set; }

        /// <summary>
        /// Gets a value indicating whether this ServiceContextConnector is currently in a state that allows it to perform service tasks
        /// </summary>
        private bool Ready
        {
            get {
                if (!client.Operational)
                {
                    if (ready.WaitOne(2000))
                    {
                        ready.Reset();
                    }
                }

                return client.Operational;
            }
        }

        public TaskModel[] GetScheduledTasks()
        {
            List<TaskModel> retVal = new List<TaskModel>();
            if (Ready)
            {
                var schedulers = manager.GetAvailableSchedulers();
                foreach (var sched in schedulers)
                {
                    if (sched.SupportsPushTask)
                    {
                        retVal.AddRange(GetScheduledTasks(sched.SchedulerName));
                    }
                }
            }

            return retVal.ToArray();
        }

        /// <summary>
        /// Gets all scheduled tasks for the configured Scheduler
        /// </summary>
        /// <param name="schedulerName">the name of the target-Scheduler that holds the requested tasks</param>
        /// <returns>a TaskModel instance containing all scheduled tasks</returns>
        public TaskModel[] GetScheduledTasks(string schedulerName)
        {
            if (Ready)
            {
                var tasks = manager.GetScheduledTasks(schedulerName);
                return (from t in tasks
                        select
                            new TaskModel
                                {
                                    TaskId = Convert.ToInt32(t.MetaData["PERIODICTASKID"]),
                                    Remarks = t.Remarks,
                                    TaskName = t.MetaData["NAME"].ToString(),
                                    LastExecution = t.LastExecution
                                }).ToArray();
            }
            else
            {
                return new TaskModel[0];
            }
        }

        /// <summary>
        /// Gets a list of active StepWorkers
        /// </summary>
        /// <returns>a list of available workers in the service</returns>
        public string[] GetActiveWorkers()
        {
            if (Ready)
            {
                return remoteEnvironment.GetStepWorkers();
            }

            return new string[0];
        }

        /// <summary>
        /// Invokes the specified task with parameters
        /// </summary>
        /// <param name="taskName">the name of the task that must be triggered</param>
        /// <param name="arguments">the start-parameters for the task</param>
        /// <returns>the result of the last step that was executed by the task-processor</returns>
        public async Task<object> InvokeWithParams(string taskName, Dictionary<string, object> arguments)
        {
            if (Ready)
            {

                return await remoteEnvironment.InvokeWithParams(taskName, arguments);
            }

            return null;
        }

        /// <summary>
        /// Gets a detail description of the provided workerName
        /// </summary>
        /// <param name="workerName">the name of the demanded worker</param>
        /// <returns>a detailed worker-description of the worker</returns>
        public WorkerDescription DescribeWorker(string workerName)
        {
            if (Ready)
            {
                return remoteEnvironment.DescribeWorker(workerName);
            }

            return null;
        }

        /// <summary>
        /// Pushes the specified task to run
        /// </summary>
        /// <param name="schedulerName">the name of the scheduler that is holding the task that needs to be triggered</param>
        /// <param name="taskId">the id of the periodicTask that needs to be pushed to run</param>
        /// <returns>a value indicating whether the task was successfully pushed</returns>
        public bool RunTask(string schedulerName, int taskId)
        {
            if (Ready)
            {
                ScheduledTaskDescription target =
                    (from t in manager.GetScheduledTasks(schedulerName)
                     where Convert.ToInt32(t.MetaData["PERIODICTASKID"]) == taskId
                     select t).FirstOrDefault();
                if (target != null)
                {
                    return manager.PushRequest(schedulerName, target.RequestId);
                }
            }

            return false;
        }

        /// <summary>
        /// Raises the Disposed event
        /// </summary>
        protected virtual void OnDisposed()
        {
            Disposed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Führt anwendungsspezifische Aufgaben durch, die mit der Freigabe, der Zurückgabe oder dem Zurücksetzen von nicht verwalteten Ressourcen zusammenhängen.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            OnDisposed();
        }

        #region Implementation of IPlugin

        /// <summary>
        /// Informs a calling class of a Disposal of this Instance
        /// </summary>
        public event EventHandler Disposed;

        #endregion
    }
}
