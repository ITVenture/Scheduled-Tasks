using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace PeriodicTasks.AppConfigBasedTaskLoading.Config
{
    [Serializable]
    public class StepDefinition
    {
        /// <summary>
        /// Initializes a new instance of the StepParameterCollection class
        /// </summary>
        public StepDefinition()
        {
            Parameters = new StepParameterCollection();
        }

        /// <summary>
        /// Gets or sets the Name of this step
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Command that is passed to the Worker
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// Gets or sets a condition that must be met to make this step run
        /// </summary>
        public string RunCondition { get; set; }

        /// <summary>
        /// Gets or sets the Name of the Worker that will be used to execute this step
        /// </summary>
        public string WorkerName { get; set; }

        /// <summary>
        /// Gets or sets the Order of this step. Steps will be sorted by their order, starting at the smallest
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the Variable into which the Output of this step will be saved
        /// </summary>
        public string OutputVariable { get; set; }

        /// <summary>
        /// Gets or sets an exclusive AreaName that is used for this step to ensure an exclusive execution.
        /// </summary>
        public string ExclusiveAreaName { get; set; }

        /// <summary>
        /// Gets or sets a list of Parameters that are used for this Step
        /// </summary>
        public StepParameterCollection Parameters { get; set; }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                return $"{Name} (@{WorkerName}, {Parameters.Count} parameters)";
            }

            return base.ToString();
        }
    }
}