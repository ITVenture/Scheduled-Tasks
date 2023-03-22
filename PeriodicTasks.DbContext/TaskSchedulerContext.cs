using System;
using ITVComponents.EFRepo.DIIntegration;
using ITVComponents.EFRepo.Extensions;
using ITVComponents.EFRepo.Options;
using ITVComponents.Plugins.DatabaseDrivenConfiguration.Models;
using ITVComponents.WebCoreToolkit.EntityFramework.Options;
using ITVComponents.WebCoreToolkit.Security;
using ITVComponents.WebCoreToolkit.WebPlugins.InjectablePlugins;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PeriodicTasks.DatabaseDrivenTaskLoading.Models;

namespace PeriodicTasks.DbContext
{
    [ScopedDependency(FriendlyName = "TaskSchedulerContext")]
    public class TaskSchedulerContext:Microsoft.EntityFrameworkCore.DbContext, ITaskSchedulerContext
    {
        private readonly IPermissionScope targetScope;
        private readonly DbContextModelBuilderOptions<TaskSchedulerContext> modelOptions;

        public TaskSchedulerContext(DbContextOptions options, DbContextModelBuilderOptions<TaskSchedulerContext> modelOptions, IPermissionScope targetScope) : base(options)
        {
            this.targetScope = targetScope;
            this.modelOptions = modelOptions;
        }

        public TaskSchedulerContext(ContextOptionsLoader<TaskSchedulerContext> dbOptions, IPermissionScope targetScope, bool useTenantFilter, IOptions<DbContextModelBuilderOptions<TaskSchedulerContext>> modelOptions) 
            : this(dbOptions.Options, modelOptions.Value, targetScope)
        {
            this.targetScope = targetScope;
            UseTenantFilter = useTenantFilter;
            this.modelOptions = modelOptions.Value;
            this.modelOptions.ConfigureExpressionProperty(() => CurrentTenant);
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
            modelOptions.ConfigureModelBuilder(modelBuilder);
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
