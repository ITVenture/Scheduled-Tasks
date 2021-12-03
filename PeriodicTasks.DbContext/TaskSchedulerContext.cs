using System;
using ITVComponents.EFRepo.Extensions;
using ITVComponents.Plugins.DatabaseDrivenConfiguration.Models;
using ITVComponents.WebCoreToolkit.WebPlugins.InjectablePlugins;
using Microsoft.EntityFrameworkCore;
using PeriodicTasks.DatabaseDrivenTaskLoading.Models;

namespace PeriodicTasks.DbContext
{
    [ScopedDependency(FriendlyName = "TaskSchedulerContext")]
    public class TaskSchedulerContext:Microsoft.EntityFrameworkCore.DbContext, ITaskSchedulerContext
    {
        public TaskSchedulerContext(DbContextOptions options) : base(options)
        {
        }

        public TaskSchedulerContext(string connectionString) : base(new DbContextOptionsBuilder().UseSqlServer(connectionString).Options)
        {

        }
        
        /// <summary>Gets or sets the UniqueName of this Plugin</summary>
        public string UniqueName { get; set; }

        public DbSet<PeriodicLog> PeriodicLog{ get; set; }

        public DbSet<PeriodicMonthday> PeriodicMonthdays { get;set; }

        public DbSet<PeriodicMonth> PeriodicMonths { get; set; }

        public DbSet<PeriodicRun> PeriodicRuns { get; set; }

        public DbSet<PeriodicSchedule> PeriodicSchedules { get; set; }

        public DbSet<PeriodicStepParameter> PeriodicStepParameters { get; set; }

        public DbSet<PeriodicStep> PeriodicSteps { get; set; }

        public DbSet<DatabaseDrivenTaskLoading.Models.PeriodicTask> PeriodicTasks { get; set; }

        public DbSet<PeriodicTime> PeriodicTimes { get; set; }

        public DbSet<PeriodicWeekday> PeriodicWeekDays { get; set; }

        public DbSet<DatabasePlugin> Plugins { get;set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.TableNamesFromProperties(this);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseLazyLoadingProxies();
        }

        /// <summary>
        ///     Releases the allocated resources for this context.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            OnDisposed();
        }

        /// <summary>
        /// Raises the Dispsoed event
        /// </summary>
        protected virtual void OnDisposed()
        {
            Disposed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Informs a calling class of a Disposal of this Instance
        /// </summary>
        public event EventHandler Disposed;

    }
}
