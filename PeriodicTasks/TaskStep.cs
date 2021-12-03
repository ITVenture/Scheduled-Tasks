using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeriodicTasks
{
    /// <summary>
    /// Defines a Step of a task
    /// </summary>
    [Serializable]
    public class TaskStep
    {
        /// <summary>
        /// Gets or sets the name of this step
        /// </summary>
        public string StepName { get; set; }

        /// <summary>
        /// Gets or sets the desired worker - Name of the given step
        /// </summary>
        public string StepWorkerName { get; set; }

        /// <summary>
        /// Gets or sets the name of the variable that stores the output of this step
        /// </summary>
        public string OutputVariable { get; set; }

        /// <summary>
        /// Gets or sets the command (or a command set) for the StepWorker of this step
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// Gets or sets a condition that must be met to make this step run
        /// </summary>
        public string RunCondition { get; set; }

        /// <summary>
        /// Gets or sets the name of an exclusive Execution Area. If Set, at the beginning of the Step, a NamedLock object is created and disposed at the end of the Execution.
        /// </summary>
        public string ExclusiveAreaName { get; set; }

        /// <summary>
        /// Gets or sets parameters that need to be passed to the worker that executes this TaskStep
        /// </summary>
        public StepParameter[] Parameters { get; set; }

        /// <summary>
        /// Gets or sets the Order of this step. The Steps will be sorted by the order of the steps starting at the lowest value
        /// </summary>
        public int Order { get; set; }
    }
}
