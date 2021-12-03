using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeriodicTasks
{
    /// <summary>
    /// Declares a Parameter that is passed to a specific Step of a periodic task
    /// </summary>
    public class StepParameter
    {
        /// <summary>
        /// Gets or sets the name of this Parameter
        /// </summary>
        public string ParameterName { get; set; }

        /// <summary>
        /// Gets or sets the ParameterType of this Parameter
        /// </summary>
        public ParameterType ParameterType { get; set; }

        /// <summary>
        /// Gets or sets the Value of this StepParameter
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets domain-specific settings for the worker that is processing a job in the current step
        /// </summary>
        public string ParameterSettings { get; set; }
    }

    /// <summary>
    /// Defines the Value - Source of a StepParameter
    /// </summary>
    public enum ParameterType
    {
        /// <summary>
        /// Defines a literal that will be passed to the worker as-is
        /// </summary>
        Literal,

        /// <summary>
        /// Defines an Expression that will be evalued using the ExpressionParser class
        /// </summary>
        Expression,

        /// <summary>
        /// Defines that the value will be taken from the set of results of previous steps
        /// </summary>
        Variable
    }
}
