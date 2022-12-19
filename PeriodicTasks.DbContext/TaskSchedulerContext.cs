using System;
using ITVComponents.EFRepo.Extensions;
using ITVComponents.Plugins.DatabaseDrivenConfiguration.Models;
using ITVComponents.WebCoreToolkit.Security;
using ITVComponents.WebCoreToolkit.WebPlugins.InjectablePlugins;
using Microsoft.EntityFrameworkCore;
using PeriodicTasks.DatabaseDrivenTaskLoading.Models;

namespace PeriodicTasks.DbContext
{
    [ScopedDependency(FriendlyName = "TaskSchedulerContext")]
    public class TaskSchedulerContext:Microsoft.EntityFrameworkCore.DbContext, ITaskSchedulerContext
    {
        private readonly IPermissionScope targetScope;

        public TaskSchedulerContext(DbContextOptions options, IPermissionScope targetScope) : base(options)
        {
            this.targetScope = targetScope;
        }

        public TaskSchedulerContext(string connectionString, IPermissionScope targetScope, bool useTenantFilter) : this(new DbContextOptionsBuilder().UseSqlServer(connectionString).Options, targetScope)
        {
            UseTenantFilter = useTenantFilter;
        }

        public string CurrentTenant => UseTenantFilter ? targetScope?.PermissionPrefix : null;

        public bool UseTenantFilter { get; set; }
        
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
            modelBuilder.Entity<PeriodicLog>().HasQueryFilter(n => n.TenantId == CurrentTenant);
            modelBuilder.Entity<PeriodicMonthday>().HasQueryFilter(n => n.TenantId == CurrentTenant);
            modelBuilder.Entity<PeriodicMonth>().HasQueryFilter(n => n.TenantId == CurrentTenant);
            modelBuilder.Entity<PeriodicRun>().HasQueryFilter(n => n.TenantId == CurrentTenant);
            modelBuilder.Entity<PeriodicSchedule>().HasQueryFilter(n => n.TenantId == CurrentTenant);
            modelBuilder.Entity<PeriodicStepParameter>().HasQueryFilter(n => n.TenantId == CurrentTenant);
            modelBuilder.Entity<PeriodicStep>().HasQueryFilter(n => n.TenantId == CurrentTenant);
            modelBuilder.Entity<DatabaseDrivenTaskLoading.Models.PeriodicTask>().HasQueryFilter(n => n.TenantId == CurrentTenant);
            modelBuilder.Entity<PeriodicTime>().HasQueryFilter(n => n.TenantId == CurrentTenant);
            modelBuilder.Entity<PeriodicWeekday>().HasQueryFilter(n => n.TenantId == CurrentTenant);
            modelBuilder.Entity<DatabasePlugin>().HasQueryFilter(n => n.TenantId == CurrentTenant);
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
