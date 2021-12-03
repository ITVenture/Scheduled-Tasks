using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeriodicTasks.ProcessWatchDog.InterProcess
{
    public interface IWatchDogTaskIntegration
    {
        /// <summary>
        /// Manages all processes that have registered on this watchDog instance
        /// </summary>
        ///<param name="currentTask">the task that is currently executed by the attached worker</param>
        void ManageProcesses(PeriodicTask task);
    }
}
