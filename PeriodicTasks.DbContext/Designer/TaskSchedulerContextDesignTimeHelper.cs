using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PeriodicTasks.DbContext.Designer
{
    public class TaskSchedulerContextDesignTimeHelper:IDesignTimeDbContextFactory<TaskSchedulerContext>
    {
        public TaskSchedulerContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TaskSchedulerContext>();
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=IWCSecurity;Trusted_Connection=True;");

            return new TaskSchedulerContext(optionsBuilder.Options, null);
        }
    }
}
