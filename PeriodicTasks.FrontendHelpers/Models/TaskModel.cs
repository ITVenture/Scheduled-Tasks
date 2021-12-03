using System;

namespace PeriodicTasks.FrontendHelpers.Models
{
    [Serializable]
    public class TaskModel
    {
        public int TaskId { get; set; }

        public string TaskName { get; set; }

        public string Remarks { get; set; }

        public DateTime? LastExecution { get; set; }
    }
}
