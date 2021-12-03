using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ITVComponents.DataAccess.Models;

namespace PeriodicTasks.DatabaseDrivenTaskLoading.MetaData
{
    public interface IPeriodicSchedule
    {
        [DbColumn("SchedulerName")]
        string SchedulerName { get; set; }

        [DbColumn("PeriodicScheduleId", ValueResolveExpression = @"'PeriodicTasks.DatabaseDrivenTaskLoading.Helpers.ScheduleInstructionResolver@@""PeriodicTasks.DatabaseDrivenTaskLoading.dll""'.GetSchedulerInstruction(value)")]
        string SchedulerInstruction { get; set; }
    }
}
