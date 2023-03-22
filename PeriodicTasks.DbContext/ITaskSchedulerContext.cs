using ITVComponents.Plugins;
using ITVComponents.Plugins.DatabaseDrivenConfiguration.Models;
using Microsoft.EntityFrameworkCore;
using PeriodicTasks.DatabaseDrivenTaskLoading.Models;

namespace PeriodicTasks.DbContext
{
    public interface ITaskSchedulerContext:IPlugin
    {
        string CurrentTenant { get; }

        DbSet<PeriodicLog> PeriodicLog{ get; set; }

        DbSet<PeriodicMonthday> PeriodicMonthdays { get;set; }

        DbSet<PeriodicMonth> PeriodicMonths { get; set; }

        DbSet<PeriodicRun> PeriodicRuns { get; set; }

        DbSet<PeriodicSchedule> PeriodicSchedules { get; set; }

        DbSet<PeriodicStepParameter> PeriodicStepParameters { get; set; }

        DbSet<PeriodicStep> PeriodicSteps { get; set; }

        DbSet<DatabaseDrivenTaskLoading.Models.PeriodicTask> PeriodicTasks { get; set; }

        DbSet<PeriodicTime> PeriodicTimes { get; set; }

        DbSet<PeriodicWeekday> PeriodicWeekDays { get; set; }

        DbSet<DatabasePlugin> Plugins { get; set; }

        int SaveChanges();
    }
}
