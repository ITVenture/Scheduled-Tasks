using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ITVComponents.DataAccess;
using ITVComponents.DataAccess.Models;
using ITVComponents.DataAccess.Parallel;
using ITVComponents.Helpers;
using ITVComponents.Logging;
using ITVComponents.ParallelProcessing;
using PeriodicTasks.DatabaseDrivenTaskLoading.Helpers;
using PeriodicTasks.DatabaseDrivenTaskLoading.MetaData;
using PeriodicTasks.DatabaseDrivenTaskLoading.RecordExtensions;

namespace PeriodicTasks.DatabaseDrivenTaskLoading
{
    public class DatabaseLoader : ITaskLoader
    {
        /// <summary>
        /// provides access to the database
        /// </summary>
        private IConnectionBuffer connector;

        private readonly string tenantName;

        /// <summary>
        /// Initializes a new instance of the DatabaseLoader class
        /// </summary>
        /// <param name="connector">the database connection providing access to defined tasks</param>
        public DatabaseLoader(IConnectionBuffer connector):this(connector, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the DatabaseLoader class
        /// </summary>
        /// <param name="connector">the database connection providing access to defined tasks</param>
        /// <param name="tenantName">the tenant-name when taskScheduler is used in a multi-tenant environment</param>
        public DatabaseLoader(IConnectionBuffer connector, string tenantName)
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
                IDbWrapper database;
                using (connector.AcquireConnection(false, out database))
                {
                    using (ScheduleInstructionResolver.BeginAutoResolve(connector))
                    {
                        IDbDataParameter priorityParam = database.GetParameter("priority", priority);
                        IDbDataParameter periodicTaskIdParam = database.GetParameter("periodicTaskId", 0);
                        IDbDataParameter periodicStepIdParam = database.GetParameter("periodicStepId", 0);
                        IDbDataParameter tenantParam = database.GetParameter("tenantId",
                            !string.IsNullOrEmpty(this.tenantName) ? this.tenantName : DBNull.Value);
                        DynamicResult[] original =
                            database.GetNativeResults(
                                $@"Select * from PeriodicTasks where priority = {priorityParam.ParameterName} and
({tenantParam.ParameterName}=tenantId or ({tenantParam.ParameterName} is null and tenantId is null))",
                                null,
                                priorityParam,tenantParam);
                        foreach (DynamicResult result in original)
                        {
                            Dictionary<string, object> metaData =
                                new Dictionary<string, object>();
                            metaData.Add("PERIODICTASKID", result["periodicTaskId"]);
                            PeriodicTask tmp = getTask(result["Name"], metaData);
                            try
                            {
                                using (var lk = tmp.DemandExclusive())
                                {
                                    lk.Exclusive(() =>
                                    {
                                        result.ToModel<PeriodicTask, IPeriodicTask>(tmp);
                                        periodicTaskIdParam.Value = result["periodicTaskId"];
                                        SchedulerPolicy[] schedules =
                                            database.GetResults<SchedulerPolicy, IPeriodicSchedule>(
                                                $@"Select PeriodicScheduleId, SchedulerName from PeriodicSchedules where PeriodicTaskId = {periodicTaskIdParam.ParameterName} and
({tenantParam.ParameterName}=tenantId or ({tenantParam.ParameterName} is null and tenantId is null))",
                                                periodicTaskIdParam, tenantParam).ToArray();
                                        tmp.ConfigureSchedule(schedules);
                                        DynamicResult[] steps =
                                            database.GetNativeResults(
                                                $@"Select * from PeriodicSteps where periodicTaskId = {periodicTaskIdParam.ParameterName} and
({tenantParam.ParameterName}=tenantId or ({tenantParam.ParameterName} is null and tenantId is null)) order by StepOrder", null, periodicTaskIdParam, tenantParam);
                                        TaskStep[] taskSteps = steps.GetModelResult<DbTaskStep, ITaskStep>().ToArray();
                                        for (int i = 0; i < steps.Length; i++)
                                        {
                                            periodicStepIdParam.Value = steps[i]["PeriodicStepId"];
                                            taskSteps[i].Parameters =
                                                database.GetResults<StepParameter, IStepParameter>(
                                                        $@"Select * from PeriodicStepParameters where PeriodicStepId = {periodicStepIdParam.ParameterName} and ParameterName not like '##%' and
({tenantParam.ParameterName}=tenantId or ({tenantParam.ParameterName} is null and tenantId is null))", periodicStepIdParam, tenantParam)
                                                    .ToArray();
                                            var runCondition = database.GetResults<StepParameter, IStepParameter>(
                                                    $@"Select * from PeriodicStepParameters where PeriodicStepId = {periodicStepIdParam.ParameterName} and ParameterName = '##RUNCONDITION' and
({tenantParam.ParameterName}=tenantId or ({tenantParam.ParameterName} is null and tenantId is null))", periodicStepIdParam, tenantParam)
                                                .FirstOrDefault();
                                            taskSteps[i].RunCondition = runCondition?.Value;
                                        }

                                        tmp.Steps = taskSteps;
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
                IDbWrapper database;
                using (connector.AcquireConnection(false, out database))
                {
                    IDbDataParameter nameParam = database.GetParameter("name", name);
                    IDbDataParameter periodicTaskIdParam = database.GetParameter("periodicTaskId", 0);
                    IDbDataParameter periodicStepIdParam = database.GetParameter("periodicStepId", 0);
                    IDbDataParameter tenantParam = database.GetParameter("tenantId",
                        !string.IsNullOrEmpty(this.tenantName) ? this.tenantName : DBNull.Value);

                    DynamicResult[] original =
                        database.GetNativeResults(
                            $@"Select * from PeriodicTasks where name = {nameParam.ParameterName} and
({tenantParam.ParameterName}=tenantId or ({tenantParam.ParameterName} is null and tenantId is null))",
                            null,
                            nameParam, tenantParam);
                    DynamicResult result = original.FirstOrDefault();
                    if (result != null)
                    {
                        Dictionary<string, object> metaData =
                            new Dictionary<string, object>();
                        metaData.Add("PERIODICTASKID", result["periodicTaskId"]);
                        PeriodicTask tmp = getTask(result["Name"], metaData);
                        lock (tmp)
                        {

                            result.ToModel<PeriodicTask, IPeriodicTask>(tmp);
                            tmp.SingleRun = true;
                            tmp.Active = true;
                            periodicTaskIdParam.Value = result["periodicTaskId"];
                            DynamicResult[] steps =
                                database.GetNativeResults(
                                    $@"Select * from PeriodicSteps where periodicTaskId = {periodicTaskIdParam.ParameterName} and
({tenantParam.ParameterName}=tenantId or ({tenantParam.ParameterName} is null and tenantId is null)) order by StepOrder", null, periodicTaskIdParam, tenantParam);
                            TaskStep[] taskSteps = steps.GetModelResult<DbTaskStep, ITaskStep>().ToArray();
                            for (int i = 0; i < steps.Length; i++)
                            {
                                periodicStepIdParam.Value = steps[i]["PeriodicStepId"];
                                taskSteps[i].Parameters =
                                    database.GetResults<StepParameter, IStepParameter>(
                                        $@"Select * from PeriodicStepParameters where PeriodicStepId = {periodicStepIdParam.ParameterName} and ParameterName not like '##%' and
({tenantParam.ParameterName}=tenantId or ({tenantParam.ParameterName} is null and tenantId is null))", periodicStepIdParam, tenantParam).ToArray();
                                var runCondition = database.GetResults<StepParameter, IStepParameter>(
                                    $@"Select * from PeriodicStepParameters where PeriodicStepId = {periodicStepIdParam.ParameterName} and ParameterName = '##RUNCONDITION' and
({tenantParam.ParameterName}=tenantId or ({tenantParam.ParameterName} is null and tenantId is null))", periodicStepIdParam, tenantParam).FirstOrDefault();
                                taskSteps[i].RunCondition = runCondition?.Value;
                            }

                            tmp.Steps = taskSteps;
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
