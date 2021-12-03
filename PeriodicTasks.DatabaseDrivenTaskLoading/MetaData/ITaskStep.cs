using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ITVComponents.DataAccess.Models;

namespace PeriodicTasks.DatabaseDrivenTaskLoading.MetaData
{
    public interface ITaskStep
    {
        [DbColumn("PeriodicStepId")]
        int TaskStepId { get; set; }
        [DbColumn("Name")]
        string StepName { get; set; }
        [DbColumn("WorkerName")]
        string StepWorkerName { get; set; }
        [DbColumn("OutputVariable")]
        string OutputVariable { get; set; }
        [DbColumn("Command")]
        string Command { get; set; }
        [DbColumn("StepOrder")]
        int Order { get; set; }
        [DbColumn(true)]
        string ExclusiveAreaName { get; set; }
    }
}
