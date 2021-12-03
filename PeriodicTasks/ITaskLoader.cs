using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ITVComponents.Plugins;

namespace PeriodicTasks
{
    /// <summary>
    /// Defines a Task-loader Strategy that can be used to manage tasks
    /// </summary>
    public interface ITaskLoader:IPlugin
    {
        /// <summary>
        /// Refreshes Tasks that are stored in a source that can be accessed with the implemented strategy
        /// </summary>
        /// <param name="priority">the priority for which to load tasks</param>
        /// <param name="getTask">callback for getting the raw-task for a specific name, so it can be refreshed</param>
        void RefreshTasks(int priority, Func<string, Dictionary<string,object>, PeriodicTask> getTask);

        /// <summary>
        /// Gets the Task-Description for a specific task with a run-once configuration
        /// </summary>
        /// <param name="name">the name of the requested task</param>
        /// <param name="getTask">callback for getting the raw-task for a specific name, so it can be refreshed</param>
        PeriodicTask GetRunOnceTask(string name, Func<string, Dictionary<string, object>, PeriodicTask> getTask);
    }
}
