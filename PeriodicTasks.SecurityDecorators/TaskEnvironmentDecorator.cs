using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ITVComponents.InterProcessCommunication.Shared.Security;
using ITVComponents.InterProcessCommunication.Shared.Security.PermissionBasedSecurity;
using ITVComponents.Plugins;
using PeriodicTasks.Remote;

namespace PeriodicTasks.SecurityDecorators
{
    public class TaskEnvironmentDecorator:IServiceDecorator, ITaskEnvironment
    {
        private readonly PeriodicEnvironment decorated;

        /// <summary>
        /// Initializes a new instance of the TaskEnvironmentDecorator class
        /// </summary>
        /// <param name="decorated"></param>
        public TaskEnvironmentDecorator(PeriodicEnvironment decorated)
        {
            this.decorated = decorated;
        }

        /// <summary>Gets or sets the UniqueName of this Plugin</summary>
        public string UniqueName { get; set; }

        /// <summary>The Decorated Service-Instance</summary>
        public IPlugin DecoratedService => decorated;

        /// <summary>
        /// Gets a list of workers on the connected Environment
        /// </summary>
        /// <returns>a list of present worker-Names</returns>
        [HasPermission("PeriodicTasks.EnumWorkers")]
        public string[] GetStepWorkers()
        {
            return decorated.GetStepWorkers();
        }

        /// <summary>
        /// Describes a specific worker
        /// </summary>
        /// <param name="worker">the name of the worker to describe</param>
        /// <returns>a complete description of the requrested worker</returns>
        [HasPermission("PeriodicTasks.EnumWorkers", "PeriodicTasks.GetWorkerDescriptions")]
        public WorkerDescription DescribeWorker(string worker)
        {
            return decorated.DescribeWorker(worker);
        }

        /// <summary>
        /// Invokes a task with the given name and the given arguments
        /// </summary>
        /// <param name="taskName">the task that is known to the remote environment</param>
        /// <param name="arguments">the arguments required by the task for the custom run</param>
        /// <returns>an awaitable task that ends, when the environment has processed the custom periodicTask object</returns>
        [HasPermission("PeriodicTasks.InvokeTaskTemplates")]
        public Task<object> InvokeWithParams(string taskName, Dictionary<string, object> arguments)
        {
            return decorated.InvokeWithParams(taskName, arguments);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
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
