using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace PeriodicTasks.AppConfigBasedTaskLoading.Config
{
    [Serializable]
    public class StepParameterDefinition
    {
        /// <summary>
        /// Gets or sets the Name of this Parameter
        /// </summary>
        public string ParameterName { get; set; }

        /// <summary>
        /// Gets or sets the Type of this Parameter
        /// </summary>
        public ParameterType ParameterType { get; set; }

        /// <summary>
        /// Gets or sets the Settings that are used to fill this parameter
        /// </summary>
        public string ParameterSettings { get; set; }

        /// <summary>
        /// Gets or sets the Value of this Parameter
        /// </summary>
        public string Value { get; set; }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(ParameterName))
            {
                return $"{ParameterName} ({ParameterType}, {Value})";
            }

            return base.ToString();
        }
    }
}
