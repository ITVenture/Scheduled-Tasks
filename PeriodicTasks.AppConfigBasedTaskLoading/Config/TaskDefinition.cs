using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace PeriodicTasks.AppConfigBasedTaskLoading.Config
{
    [Serializable]
    public class TaskDefinition
    {
        /// <summary>
        /// Initializes a new instance of the StepCollection class
        /// </summary>
        public TaskDefinition()
        {
            Steps = new StepCollection();
            Schedules = new SchedulerPolicyCollection();
        }

        /// <summary>
        /// Gets or sets the name of a specific task
        /// </summary>
        public string TaskName { get; set; }

        /// <summary>
        /// Gets or sets the Priority of this Task
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this task is active
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets a Description of what this job does
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets an exclusive AreaName that is used for this Task to ensure an exclusive execution.
        /// </summary>
        public string ExclusiveAreaName { get; set; }

        /// <summary>
        /// Gets or sets a list of Steps that are associated with this Task
        /// </summary>
        public StepCollection Steps { get; set; }

        /// <summary>
        /// Gets or sets a list of schedules for this TaskDefinition item
        /// </summary>
        public SchedulerPolicyCollection Schedules { get; set; }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(TaskName))
            {
                return $"{TaskName} ({(Active?"Enabled":"Disabled")}, Priority {Priority}, {Steps.Count} Steps, {Schedules.Count} Schedules)";
            }

            return base.ToString();
        }
    }
}
