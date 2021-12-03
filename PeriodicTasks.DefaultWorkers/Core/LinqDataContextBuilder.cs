using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ITVComponents.DataAccess;
using ITVComponents.DataAccess.Linq;
using PeriodicTasks.Events;

namespace PeriodicTasks.DefaultWorkers.Core
{
    /// <summary>
    /// Provides DataContexts that can be used from a LinqData-Adapter
    /// </summary>
    public class LinqDataContextBuilder : PeriodicTasksEventInterceptor, IDataContext
    {
        /// <summary>
        /// Initializes a new instance of the LinqDataContextBuilder class
        /// </summary>
        /// <param name="environment">the Environment that is executing tasks</param>
        public LinqDataContextBuilder(PeriodicEnvironment environment) : base(environment)
        {
        }

        #region ignored events
        protected override void InterceptTaskStarts(PeriodicTask target, Dictionary<string, object> variables)
        {
        }

        protected override void InterceptTaskEnds(PeriodicTask target, Dictionary<string, object> variables)
        {   
        }

        protected override void InterceptTaskEndsWithError(PeriodicTask target, Dictionary<string, object> variables)
        {   
        }

        protected override void InterceptTaskMessage(PeriodicTask target, LogMessageType messageType, string message)
        {
        }

        protected override void InterceptTaskTerminatesPlanned(PeriodicTask target)
        {
        }

        protected override void InterceptTaskTerminationDueToRunCondition(PeriodicTask target, TaskStep step, int stepId, int stepCount)
        {
        }

        #endregion

        /// <summary>
        /// Intercepts the event before-step on the provided task
        /// </summary>
        /// <param name="target">the task on which a step is about to be executed</param>
        /// <param name="step">the step that is executing</param>
        /// <param name="stepId">the sequence-identity (1..n) of the current step</param>
        /// <param name="stepCount">the total number of steps on the current task</param>
        /// <param name="variables">the variables for the given job</param>
        protected override void InterceptBeforeTaskStep(PeriodicTask target, TaskStep step, int stepId, int stepCount, Dictionary<string, object> variables)
        {

            var newDic = new Dictionary<string, IEnumerable<DynamicResult>>();
            target.SetTemporaryValue("LinqTables", newDic);
            foreach (KeyValuePair<string, object> val in variables)
            {
                DynamicResult[] value = val.Value as DynamicResult[];
                if (value != null)
                {
                    newDic.Add(val.Key, value);
                }

                if (val.Value is IDictionary<string, DynamicResult[]> nestedValue )
                {
                    foreach (KeyValuePair<string, DynamicResult[]> nestedTab in nestedValue)
                    {
                        string newName = string.Format("{0}{1}", val.Key, nestedTab.Key);
                        if (!newDic.ContainsKey(newName))
                        {
                            newDic.Add(newName, nestedTab.Value);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Intercepts the event after-step on the provided task
        /// </summary>
        /// <param name="target">the task that has finished a specific step</param>
        /// <param name="step">the step that has finished execution</param>
        /// <param name="stepId">the sequence-identity (1..n) of the current step</param>
        /// <param name="stepCount">the total number of steps on the current task</param>
        /// <param name="result">the result that was returned by the worker of the given step</param>
        /// <param name="variables">the variables of the given job</param>
        protected override void InterceptAfterTaskStep(PeriodicTask target, TaskStep step, int stepId, int stepCount, object result,
                                                       Dictionary<string, object> variables)
        {
            var dic = target.GetTemporaryValue<Dictionary<string, IEnumerable<DynamicResult>>>("LinqTables");
            if (dic != null)
            {
                dic.Clear();
            }
        }

        /// <summary>
        /// Gets a List of available Tables for this DataContext
        /// </summary>
        public IDictionary<string, IEnumerable<DynamicResult>> Tables
        {
            get
            {
                return null; /*tables.Value;*/ }
        }
    }
}
