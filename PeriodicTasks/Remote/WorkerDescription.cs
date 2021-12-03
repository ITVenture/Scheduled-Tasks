using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeriodicTasks.Remote
{
    [Serializable]
    public class WorkerDescription
    {
        /// <summary>
        /// Gets or sets the Runtime Type of the described worker
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Describes the purpose of the worker
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Describes the Expected Parameters for this worker
        /// </summary>
        public ParameterDescription[] ExpectedParameters { get; set; }

        /// <summary>
        /// Desribes the expected command formatting for the command
        /// </summary>
        public string CommandDescription { get; set; }

        /// <summary>
        /// Further remarks for the described worker
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// The Return Type of the worker
        /// </summary>
        public string ReturnType { get; set; }

        /// <summary>
        /// The description of the Return-Value
        /// </summary>
        public string ReturnDescription { get; set; }
    }
}
