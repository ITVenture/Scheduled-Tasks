using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITVComponents.Helpers;
using ITVComponents.InterProcessCommunication.Shared.WatchDogs;
namespace PeriodicTasks.ProcessWatchDog.InterProcess
{
    /// <summary>
    /// ProcessWatchdog implementation that logs the progress of stopping tasks into the TaskLog of a periodicTask run.
    /// </summary>
    public class WatchDogService:ITVComponents.InterProcessCommunication.Shared.WatchDogs.ProcessWatchDog, IWatchDogTaskIntegration
    {
        /// <summary>
        /// The Worker that is linked to this WatchdogService instance
        /// </summary>
        private WatchDogWorker targetWorker;

        /// <summary>
        /// Ensures that the ManageProcesses method is only called once at a time
        /// </summary>
        private object singleInstanceRun = new object();

        /// <summary>
        /// Holds the current Task
        /// </summary>
        private PeriodicTask currentTask;

        /// <summary>
        /// Initializes a new instance of the WatchdogService class
        /// </summary>
        /// <param name="targetWorker">the targetWorker that is used to periodically check on the watched processes</param>
        public WatchDogService(WatchDogWorker targetWorker)
        {
            this.targetWorker = targetWorker;
            targetWorker.RegisterWatchDog(this);
        }

        /// <summary>
        /// Manages all processes that have registered on this watchDog instance
        /// </summary>
        ///<param name="currentTask">the task that is currently executed by the attached worker</param>
        public void ManageProcesses(PeriodicTask currentTask)
        {
            lock (singleInstanceRun)
            {
                this.currentTask = currentTask;
                try
                {
                    ManageProcesses();
                }
                finally
                {
                    this.currentTask = null;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the given process exists
        /// </summary>
        /// <param name="machineName">the name of the machine executing the requested process</param>
        /// <param name="processId">the id of the process</param>
        /// <param name="processName">the name of the requested process</param>
        /// <param name="process">the instance of the existing process</param>
        /// <returns>a value indicating whether the given process exists and is alive</returns>
        protected override bool ProcessAlive(string machineName, int processId, string processName, out Process process)
        {
            var retVal = base.ProcessAlive(machineName, processId, processName, out process);
            currentTask?.Log($"The ProcessAlive - Check for the process {processId}({processName}) on {machineName} resulted to {retVal}.", LogMessageType.Report);
            return retVal;
        }

        /// <summary>
        /// Stops a process and removes it from the list of known processes
        /// </summary>
        /// <param name="process">the id of the process to stop</param>
        /// <param name="statusValues">a list of known processes</param>
        protected override void StopProcess(Process process, ConcurrentDictionary<int, ProcessStatus> statusValues)
        {
            currentTask?.Log($"Attempting to stop the process {process.Id}({process.ProcessName}) on {process.MachineName}...", LogMessageType.Report);
            base.StopProcess(process, statusValues);
            currentTask?.Log($"Done.", LogMessageType.Report);
        }

        /// <summary>Is called when an error occurrs</summary>
        /// <param name="action">the action that was called</param>
        /// <param name="processId">the id of the process</param>
        /// <param name="name">the name of the process</param>
        /// <param name="machine">the machine on which the process is executed</param>
        /// <param name="expected">Indicates whether the occurred error was expected</param>
        /// <param name="ex">the exception that was catched</param>
        protected override void OnError(string action, int processId, string name, string machine, bool expected, Exception ex)
        {
            base.OnError(action, processId, name, machine, expected, ex);
            if (!expected)
            {
                currentTask?.Log($@"Error on Action {action} for Process {processId}({name}) on {machine}:
{ex.OutlineException()}", LogMessageType.Error);
            }
        }
    }
}
