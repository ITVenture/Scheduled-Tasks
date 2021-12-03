using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITVComponents.ParallelProcessing;
using ITVComponents.Scripting.CScript.Core;
using ITVComponents.Scripting.CScript.Helpers;
using ITVComponents.Threading;
using PeriodicTasks.Events;
using Mutex = System.Threading.Mutex;

namespace PeriodicTasks
{
    public class PeriodicTaskProcessor : TaskWorkerBase<PeriodicTask>
    {
        /// <summary>
        /// The periodic environment that is used in order to intercept task events
        /// </summary>
        private PeriodicEnvironment environment;

        /// <summary>
        /// the current Task of this worker
        /// </summary>
        private PeriodicTask currentTask;

        /// <summary>
        /// Initializes a new instance of the PeriodicTaskProcessor class
        /// </summary>
        /// <param name="environment">the environment that hosts this processor</param>
        internal PeriodicTaskProcessor(PeriodicEnvironment environment):this()
        {
            this.environment = environment;
        }

        /// <summary>
        /// Prevents a default instance of the PeriodicTaskProcessor class from being created
        /// </summary>
        private PeriodicTaskProcessor()
        {
        }

        /// <summary>
        /// Gets the current Task that is being processed by this TaskWorker instance
        /// </summary>
        public override ITask CurrentTask => currentTask;

        /// <summary>Processes a specific task</summary>
        /// <param name="task">a task that was fetched from a priority queue</param>
        public override void Process(PeriodicTask task)
        {
            AsyncHelpers.RunSync(async () => await ProcessAsync(task));
        }

        /// <summary>
        /// Processes a specific task
        /// </summary>
        /// <param name="task">a task that was fetched from a priority queue</param>
        public async override Task ProcessAsync(PeriodicTask task)
        {
            if (task != null)
            {
                task.ClearLastResult();
                Dictionary<string, object> variables = new Dictionary<string, object>();
                if (task.SingleRun)
                {
                    task.InitSingleRun(variables);
                }

                PeriodicTasksEventInterceptor.InterceptTaskStarts(environment,task,variables);
                NamedLock @lock = null;
                if (!string.IsNullOrEmpty(task.ExclusiveAreaName))
                {
                    @lock = new NamedLock(task.ExclusiveAreaName);
                }

                try
                {
                    TaskStep[] steps;
                    lock (task)
                    {
                        steps = task.Steps;
                    }

                    Dictionary<StepParameter, object> values = new Dictionary<StepParameter, object>();
                    int stepCount = steps.Count();
                    int currentStep = 1;
                    object tmp = null;
                    foreach (TaskStep step in steps)
                    {
                        tmp = null;
                        try
                        {
                            if ((bool) ExpressionParser.Parse(step.RunCondition ?? "true", variables, a => { DefaultCallbacks.PrepareDefaultCallbacks(a.Scope, a.ReplSession); }))
                            {
                                PeriodicTasksEventInterceptor.InterceptBeforeTaskStep(environment, task, step,
                                    currentStep,
                                    stepCount, variables);
                                values.Clear();
                                foreach (StepParameter param in step.Parameters)
                                {
                                    values.Add(param, GetValue(task, param, variables));
                                }


                                StepWorker worker = StepWorker.GetWorker(step.StepWorkerName);
                                if (worker == null)
                                {
                                    throw new Exception("Requested worker is not available. Check Configuration");
                                }

                                NamedLock innerLock = null;
                                if (!string.IsNullOrEmpty(step.ExclusiveAreaName))
                                {
                                    innerLock = new NamedLock(step.ExclusiveAreaName);
                                }

                                try
                                {
                                    tmp = await worker.Run(task, step.Command, values);
                                }
                                finally
                                {
                                    innerLock?.Dispose();
                                }

                                if (tmp is PlannedTermination)
                                {
                                    PeriodicTasksEventInterceptor.InterceptTaskTerminatesPlanned(environment, task);
                                    break;
                                }
                            }
                            else
                            {
                                PeriodicTasksEventInterceptor.InterceptTaskTerminationDueToRunCondition(environment,
                                    task, step, currentStep, stepCount);
                                break;
                            }
                        }
                        finally
                        {
                            variables[step.OutputVariable] = tmp;
                            PeriodicTasksEventInterceptor.InterceptAfterTaskStep(environment, task, step, currentStep,
                                stepCount, tmp, variables);
                            currentStep++;
                        }
                    }

                    task.LastResult = tmp;
                    task.Succeeded();
                }
                catch (Exception ex)
                {
                    task.Fail(ex);
                }
                finally
                {
                    @lock?.Dispose();
                    if (!task.Success)
                    {
                        PeriodicTasksEventInterceptor.InterceptTaskEndsWithError(environment, task, variables);
                    }

                    PeriodicTasksEventInterceptor.InterceptTaskEnds(environment, task, variables);
                    if (!task.SingleRun && !environment.Requeue(task))
                    {
                        task.Fail("Unable to requeue Task");
                    }
                }
            }
        }

        /// <summary>
        /// Führt anwendungsspezifische Aufgaben durch, die mit der Freigabe, der Zurückgabe oder dem Zurücksetzen von nicht verwalteten Ressourcen zusammenhängen.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public override void Dispose()
        {
        }

        /// <summary>Resets this worker to its initial state</summary>
        public override void Reset()
        {
        }

        /// <summary>
        /// Quits this worker. If the worker is not shared between processors, this method should call also call Dispose
        /// </summary>
        public override void Quit()
        {
            Dispose();
        }

        /// <summary>
        /// Evaluates the value of a specific variable
        /// </summary>
        /// <param name="task">the task that is currently being executed</param>
        /// <param name="stepParameter">the step - parameter that is evaluated</param>
        /// <param name="variables">the variables that have been evaluated in previous steps</param>
        /// <returns>the resulting value of the provided parameter</returns>
        private object GetValue(PeriodicTask task, StepParameter stepParameter, Dictionary<string, object> variables)
        {
            switch (stepParameter.ParameterType)
            {
                case ParameterType.Literal:
                    {
                        return stepParameter.Value;
                    }
                    case ParameterType.Expression:
                    {
                        var retVal = ExpressionParser.Parse(stepParameter.Value, variables, a => { DefaultCallbacks.PrepareDefaultCallbacks(a.Scope, a.ReplSession); });
                        return retVal;
                    }
                    case ParameterType.Variable:
                    {
                        if (!variables.ContainsKey(stepParameter.Value))
                        {
                            task.Log(string.Format("The Variable {0} is not set. Returning null instead.",stepParameter.Value),LogMessageType.Warning);
                            return null;
                        }

                        return variables[stepParameter.Value];
                    }
            }

            throw new ArgumentException("An invalid Parameter type was provided");
        }
    }
}
