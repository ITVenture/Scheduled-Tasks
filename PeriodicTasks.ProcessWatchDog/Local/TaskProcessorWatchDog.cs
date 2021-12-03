using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITVComponents.InterProcessCommunication.Shared.Base;
using ITVComponents.InterProcessCommunication.Shared.WatchDogs;
using ITVComponents.ParallelProcessing;
using ITVComponents.ParallelProcessing.WatchDogs;
using ITVComponents.Plugins;

namespace PeriodicTasks.ProcessWatchDog.Local
{
    /// <summary>
    /// Task Watchdog that sets timed out tasks to failed
    /// </summary>
    public class TaskProcessorWatchDog:TimeoutWatchdog
    {
        /// <summary>
        /// Initializes a new instance of the TaskProcessorWatchDog class
        /// </summary>
        /// <param name="timeout">the timeout after which a not-responding worker will be killed</param>
        public TaskProcessorWatchDog(int timeout) : base(timeout)
        {
        }

        /// <summary>
        /// Enables a derived class to perform checks on the Task that caused the freeze of a worker
        /// </summary>
        /// <param name="task">the Task that presumably caused a worker to freeze.</param>
        protected override void HandleFailedTask(ITask task)
        {
            base.HandleFailedTask(task);
            if (task is PeriodicTask pd)
            {
                pd.Fail("The execution of the Task took too long. The process was aborted!");
            }
        }
    }
}
