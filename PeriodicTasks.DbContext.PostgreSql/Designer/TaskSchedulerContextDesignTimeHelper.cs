using System.Runtime.InteropServices;
using ITVComponents.EFRepo.Options;
using ITVComponents.WebCoreToolkit.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PeriodicTasks.DbContext.PostgreSql.Designer
{
    public class TaskSchedulerContextDesignTimeHelper:IDesignTimeDbContextFactory<TaskSchedulerContext>
    {
        public TaskSchedulerContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TaskSchedulerContext>();
            optionsBuilder.UseNpgsql(so =>
                so.MigrationsAssembly(typeof(TaskSchedulerContextDesignTimeHelper).Assembly.FullName));
            return new TaskSchedulerContext(optionsBuilder.Options, new DbContextModelBuilderOptions<TaskSchedulerContext>(), null);
        }
    }
}
