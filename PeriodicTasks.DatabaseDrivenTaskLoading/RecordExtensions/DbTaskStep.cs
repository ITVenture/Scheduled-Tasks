using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeriodicTasks.DatabaseDrivenTaskLoading.RecordExtensions
{
    [Serializable]
    public class DbTaskStep:TaskStep
    {
        /// <summary>
        /// Gets or sets the TaskStep - ID of this Task Step
        /// </summary>
        public int TaskStepId { get; set; }
    }
}
