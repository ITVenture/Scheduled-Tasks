using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ITVComponents.Plugins;

namespace PeriodicTasks
{
    /// <summary>
    /// Possible Message Types of a log - message
    /// </summary>
    public enum LogMessageType
    {
        /// <summary>
        /// this is a normal report, indicating progress of the current task
        /// </summary>
        Report,

        /// <summary>
        /// Warning message saying that the task is not performing as expected
        /// </summary>
        Warning,

        /// <summary>
        /// Error message, saying that the task is going to fail
        /// </summary>
        Error
    }
}
