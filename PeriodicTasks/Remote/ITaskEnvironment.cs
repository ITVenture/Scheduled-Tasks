using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeriodicTasks.Remote
{
    public interface ITaskEnvironment
    {
        /// <summary>
        /// Gets a list of workers on the connected Environment
        /// </summary>
        /// <returns>a list of present worker-Names</returns>
        string[] GetStepWorkers();

        /// <summary>
        /// Describes a specific worker
        /// </summary>
        /// <param name="worker">the name of the worker to describe</param>
        /// <returns>a complete description of the requrested worker</returns>
        WorkerDescription DescribeWorker(string worker);

        /// <summary>
        /// Invokes a task with the given name and the given arguments
        /// </summary>
        /// <param name="taskName">the task that is known to the remote environment</param>
        /// <param name="arguments">the arguments required by the task for the custom run</param>
        /// <returns>an awaitable task that ends, when the environment has processed the custom periodicTask object</returns>
        Task<object> InvokeWithParams(string taskName, Dictionary<string, object> arguments);
    }
}
