using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ITVComponents.DataAccess;
using ITVComponents.DataAccess.Extensions;
using ITVComponents.DataAccess.Models;
using ITVComponents.DataAccess.Parallel;
using ITVComponents.EFRepo;
using ITVComponents.Logging;
using ITVComponents.ParallelProcessing;
using Microsoft.EntityFrameworkCore;
using PeriodicTasks.DatabaseDrivenTaskLoading.EFCore.Helpers;
using PeriodicTasks.DatabaseDrivenTaskLoading.Helpers;
using PeriodicTasks.DatabaseDrivenTaskLoading.MetaData;
using PeriodicTasks.DatabaseDrivenTaskLoading.Models;
using PeriodicTasks.DatabaseDrivenTaskLoading.RecordExtensions;
using PeriodicTasks.DbContext;

namespace PeriodicTasks.DatabaseDrivenTaskLoading.EFCore
{
    public class DatabaseLoader<TContext> : ITaskLoader where TContext: Microsoft.EntityFrameworkCore.DbContext, ITaskSchedulerContext
    {
        /// <summary>
        /// provides access to the database
        /// </summary>
        private IContextBuffer connector;

        private readonly string tenantName;

        /// <summary>
        /// Initializes a new instance of the DatabaseLoader class
        /// </summary>
        /// <param name="connector">the database connection providing access to defined tasks</param>
        public DatabaseLoader(IContextBuffer connector):this(connector, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the DatabaseLoader class
        /// </summary>
        /// <param name="connector">the database connection providing access to defined tasks</param>
        /// <param name="tenantName">the tenant-name when taskScheduler is used in a multi-tenant environment</param>
        public DatabaseLoader(IContextBuffer connector, string tenantName)
        {
            this.connector = connector;
            this.tenantName = tenantName;
        }

        /// <summary>
        /// Gets or sets the UniqueName of this Plugin
        /// </summary>
        public string UniqueName { get; set; }

        /// <summary>
        /// Refreshes Tasks that are stored in a source that can be accessed with the implemented strategy
        /// </summary>
        /// <param name="priority">the priority for which to load tasks</param>
        /// <param name="getTask">callback for getting the raw-task for a specific name, so it can be refreshed</param>
        public void RefreshTasks(int priority, Func<string, Dictionary<string,object>, PeriodicTask> getTask, Action<PeriodicTask> taskConfigured)
        {
            try
            {
                using (connector.AcquireContext<TContext>(out var database))
                {
                    using (ScheduleInstructionResolver<TContext>.BeginAutoResolve(connector))
                    {
                        var original =
                            database.PeriodicTasks.Where(n => n.Priority == priority).ToArray();
                        foreach (var result in original)
                        {
                            Dictionary<string, object> metaData =
                                new Dictionary<string, object>();
                            metaData.Add("PERIODICTASKID", result.PeriodicTaskId);
                            PeriodicTask tmp = getTask(result.Name, metaData);
                            try
                            {
                                using (var lk = tmp.DemandExclusive())
                                {
                                    lk.Exclusive(() =>
                                    {
                                        result.CopyToModel(tmp);//.ToModel<PeriodicTask, IPeriodicTask>(tmp);
                                        var schedules = database.PeriodicSchedules
                                            .Where(n => n.PeriodicTaskId == result.PeriodicTaskId)
                                            .Select(n => new SchedulerPolicy
                                            {
                                                SchedulerName = n.SchedulerName,
                                                SchedulerInstruction = ScheduleInstructionResolver<TContext>.GetSchedulerInstruction(n.PeriodicScheduleId)
                                            }).ToArray();
                                        tmp.ConfigureSchedule(schedules);
                                        var steps =
                                            database.PeriodicSteps.Where(n => n.PeriodicTaskId == result.PeriodicTaskId)
                                                .OrderBy(n => n.StepOrder).Select(n =>
                                                    new DbTaskStep
                                                    {
                                                        Command = n.Command,
                                                        ExclusiveAreaName = n.ExclusiveAreaName,
                                                        Order = n.StepOrder,
                                                        OutputVariable = n.OutputVariable,
                                                        StepName = n.Name,
                                                        StepWorkerName = n.WorkerName,
                                                        TaskStepId = n.PeriodicStepId,
                                                        
                                                    }).ToArray();
                                        for (int i = 0; i < steps.Length; i++)
                                        {
                                            steps[i].Parameters =
                                                database.PeriodicStepParameters.Where(n =>
                                                        n.PeriodicStepId == steps[i].TaskStepId
                                                        && !n.ParameterName.StartsWith("##"))
                                                    .Select(n => new StepParameter
                                                    {
                                                        Value = n.Value,
                                                        ParameterName = n.ParameterName,
                                                        ParameterSettings = n.Settings,
                                                        ParameterType = Enum.Parse<ParameterType>(n.ParameterType)
                                                    }).ToArray();
                                            var runCondition = database.PeriodicStepParameters
                                                .FirstOrDefault(n => n.PeriodicStepId == steps[i].TaskStepId && n.ParameterName == "##RUNCONDITION");
                                            steps[i].RunCondition = runCondition?.Value;
                                        }

                                        tmp.Steps = steps;
                                    });
                                }
                            }
                            finally
                            {
                                taskConfigured(tmp);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogEnvironment.LogEvent(ex.ToString(), LogSeverity.Error);
            }
        }

        public PeriodicTask GetRunOnceTask(string name, Func<string, Dictionary<string, object>, PeriodicTask> getTask, Action<PeriodicTask> taskConfigured)
        {
            try
            {
                using (connector.AcquireContext<TContext>(out var database))
                {
                    var item = 
                        database.PeriodicTasks.FirstOrDefault(n=> n.Name == name);
                    if (item!= null)
                    {
                        Dictionary<string, object> metaData =
                            new Dictionary<string, object>();
                        metaData.Add("PERIODICTASKID", item.PeriodicTaskId);
                        PeriodicTask tmp = getTask(item.Name, metaData);
                        lock (tmp)
                        {
                            item.CopyToModel(tmp);
                            tmp.SingleRun = true;
                            tmp.Active = true;
                            var steps =
                                            database.PeriodicSteps.Where(n => n.PeriodicTaskId == item.PeriodicTaskId)
                                                .OrderBy(n => n.StepOrder).Select(n =>
                                                    new DbTaskStep
                                                    {
                                                        Command = n.Command,
                                                        ExclusiveAreaName = n.ExclusiveAreaName,
                                                        Order = n.StepOrder,
                                                        OutputVariable = n.OutputVariable,
                                                        StepName = n.Name,
                                                        StepWorkerName = n.WorkerName,
                                                        TaskStepId = n.PeriodicStepId,

                                                    }).ToArray();
                            for (int i = 0; i < steps.Length; i++)
                            {
                                steps[i].Parameters =
                                    database.PeriodicStepParameters.Where(n =>
                                            n.PeriodicStepId == steps[i].TaskStepId
                                            && !n.ParameterName.StartsWith("##"))
                                        .Select(n => new StepParameter
                                        {
                                            Value = n.Value,
                                            ParameterName = n.ParameterName,
                                            ParameterSettings = n.Settings,
                                            ParameterType = Enum.Parse<ParameterType>(n.ParameterType)
                                        }).ToArray();
                                var runCondition = database.PeriodicStepParameters
                                    .FirstOrDefault(n => n.PeriodicStepId == steps[i].TaskStepId && n.ParameterName == "##RUNCONDITION");
                                steps[i].RunCondition = runCondition?.Value;
                            }

                            tmp.Steps = steps;
                        }

                        taskConfigured(tmp);
                        return tmp;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                LogEnvironment.LogEvent(ex.ToString(), LogSeverity.Error);
                throw;
            }
        }

        /// <summary>
        /// Führt anwendungsspezifische Aufgaben durch, die mit der Freigabe, der Zurückgabe oder dem Zurücksetzen von nicht verwalteten Ressourcen zusammenhängen.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            OnDisposed();
        }

        /// <summary>
        /// Raises the Disposed Event
        /// </summary>
        protected virtual void OnDisposed()
        {
            EventHandler handler = Disposed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Informs a calling class of a Disposal of this Instance
        /// </summary>
        public event EventHandler Disposed;
    }
}
