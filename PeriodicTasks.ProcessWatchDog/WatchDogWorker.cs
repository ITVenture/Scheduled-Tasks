using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeriodicTasks.ProcessWatchDog.InterProcess;

namespace PeriodicTasks.ProcessWatchDog
{
    public class WatchDogWorker:StepWorker
    {
        private IWatchDogTaskIntegration targetService;

        protected override object RunTask(PeriodicTask task, string command, Dictionary<StepParameter, object> values)
        {
            targetService.ManageProcesses(task); 
            return "OK";
        }

        public void RegisterWatchDog(IWatchDogTaskIntegration targetService)
        {
            if (this.targetService != null)
            {
                throw new InvalidOperationException("There is already a WatchDog attached to this Worker");
            }

            this.targetService = targetService;
        }
    }
}
