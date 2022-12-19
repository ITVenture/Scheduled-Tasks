using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using ITVComponents.DataAccess.Extensions;
using ITVComponents.Formatting;
using ITVComponents.Scripting.CScript.Core;
using ITVComponents.Scripting.CScript.Helpers;
using ITVComponents.Security;
using PeriodicTasks.Events;
using PeriodicTasks.MailLogger.Config;

namespace PeriodicTasks.MailLogger
{
    public class MailLog : PeriodicTasksEventInterceptor
    {
        /// <summary>
        /// Initializes a new instance of the MailLog class
        /// </summary>
        /// <param name="environment">the PeriodicEnvironment on which to register this logger</param>
        public MailLog(PeriodicEnvironment environment) : base(environment)
        {
        }

        /// <summary>
        /// Intercepts the Task-Started event on the provided task
        /// </summary>
        /// <param name="target">the target task that has started</param>
        /// <param name="variables">the variables of the given task</param>
        protected override void InterceptTaskStarts(PeriodicTask target, Dictionary<string, object> variables)
        {
            Log(MailEventTypes.TaskStart, BuildVariables(variables, new KeyValuePair<string, object>("Task", target)));
        }

        /// <summary>
        /// Intercepts the Task-Ended event on the provided task
        /// </summary>
        /// <param name="target">the target task that has ended</param>
        /// <param name="variables">the variables of the given task</param>
        protected override void InterceptTaskEndsWithError(PeriodicTask target, Dictionary<string, object> variables)
        {
            Log(MailEventTypes.TaskEndWithErrors, BuildVariables(variables, new KeyValuePair<string, object>("Task", target)));
        }

        /// <summary>
        /// Intercepts the Task-Ended event on the provided task
        /// </summary>
        /// <param name="target">the target task that has ended</param>
        /// <param name="variables">the variables of the given task</param>
        protected override void InterceptTaskEnds(PeriodicTask target, Dictionary<string, object> variables)
        {
            Log(MailEventTypes.TaskEnd, BuildVariables(variables, new KeyValuePair<string, object>("Task", target)));
        }

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
            Log(MailEventTypes.TaskStepStart,
                BuildVariables(variables, new KeyValuePair<string, object>("Task", target),
                               new KeyValuePair<string, object>("Step", step),
                               new KeyValuePair<string, object>("StepId", stepId),
                               new KeyValuePair<string, object>("StepCount", stepCount)));
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
        protected override void InterceptAfterTaskStep(PeriodicTask target, TaskStep step, int stepId, int stepCount, object result, Dictionary<string, object> variables)
        {
            Log(MailEventTypes.TaskStepEnd,
                BuildVariables(variables, new KeyValuePair<string, object>("Task", target),
                               new KeyValuePair<string, object>("Step", step),
                               new KeyValuePair<string, object>("StepId", stepId),
                               new KeyValuePair<string, object>("StepCount", stepCount),
                               new KeyValuePair<string, object>("Result", result)));
        }

        /// <summary>
        /// Intercepts the event when a job tries to log a message
        /// </summary>
        /// <param name="target">the target job that has generated a message</param>
        /// <param name="messageType">the type of the message</param>
        /// <param name="message">the message that was generated</param>
        protected override void InterceptTaskMessage(PeriodicTask target, LogMessageType messageType, string message)
        {
            Log(MailEventTypes.TaskMessage,
                new Dictionary<string, object> {{"Task", target}, {"MessageType", messageType}, {"Message", message}});
        }

        /// <summary>
        /// Intercepts the event when a job terminates because of a condition that did not apply
        /// </summary>
        /// <param name="target">the target job that has generated a message</param>
        protected override void InterceptTaskTerminatesPlanned(PeriodicTask target)
        {
            Log(MailEventTypes.TaskTerminationPlanned, new Dictionary<string, object> {{"Task", target}});
        }

        /// <summary>
        /// Intercepts the event when a job terminates because of the RunCondition of a Step that resulted to false
        /// </summary>
        /// <param name="target">the target job that has generated a message</param>
        /// <param name="step">the step that has caused to termination of a Task execution</param>
        /// <param name="stepId">the sequence-identity (1..n) of the current step</param>
        /// <param name="stepCount">the total number of steps on the current task</param>
        protected override void InterceptTaskTerminationDueToRunCondition(PeriodicTask target, TaskStep step, int stepId, int stepCount)
        {
            Log(MailEventTypes.TaskTerminationDueToRunCondition, new Dictionary<string, object>
            {
                {"Task",target },
                {"Step", step },
                { "StepId", stepId},
                {"StepCount", stepCount }
            });
        }

        /// <summary>
        /// Builds a Variable Dictionary that can be used by the Log-Method in order to determine whether a specific event needs to be protocolled
        /// </summary>
        /// <param name="variables">the variables that were generated by the task</param>
        /// <param name="extensions">further object to put into the variable collection</param>
        /// <returns>a dictionary containing all required variables</returns>
        private IDictionary<string, object> BuildVariables(Dictionary<string, object> variables, params KeyValuePair<string, object>[] extensions)
        {
            Dictionary<string, object> retVal = new Dictionary<string, object>();
            variables.ForEach(n => retVal.Add(n.Key, n.Value));
            extensions.ForEach(n => retVal.Add(n.Key, n.Value));
            return retVal;
        }

        /// <summary>
        /// Logs the provided event to all required targets
        /// </summary>
        /// <param name="eventType">the event-type</param>
        /// <param name="variables">the variables that can be accessed for the given event type</param>
        private void Log(MailEventTypes eventType, IDictionary<string, object> variables)
        {
            IEnumerable<MailConfiguration> configs = (from t in MailLogConfig.Helper.Schemas
                                                      where
                                                          t.Events.Any(
                                                              n =>
                                                              ((n.LoggedEventTypes & eventType) != 0) &&
                                                              (bool)
                                                              ExpressionParser.Parse(n.LogConditionExpression, variables,
                                                                  a => { DefaultCallbacks.PrepareDefaultCallbacks(a.Scope, a.ReplSession); }))
                                                      select t);
            foreach (MailConfiguration cfg in configs)
            {
                string message = string.Join("\r\n", (from t in cfg.Events
                                                      where
                                                          (t.LoggedEventTypes & eventType) != 0 &&
                                                          (bool)
                                                          ExpressionParser.Parse(t.LogConditionExpression, variables,
                                                              a => { DefaultCallbacks.PrepareDefaultCallbacks(a.Scope, a.ReplSession); })
                                                      select variables.FormatText(t.LogMessageFormat, TextFormat.DefaultFormatPolicyWithPrimitives)));
                using (SmtpClient client = new SmtpClient(cfg.MailServer, cfg.Port))
                {
                    client.EnableSsl = cfg.UseSSL;
                    if (cfg.UseSSL)
                    {
                        System.Net.ServicePointManager.ServerCertificateValidationCallback =
                            (sender, certificate, chain, errors) => true;
                    }
                    if (cfg.AuthenticationRequired)
                    {
                        client.UseDefaultCredentials = false;

                        client.Credentials = new NetworkCredential(cfg.MailUser,
                            cfg.MailPassword.Decrypt().Secure());
                    }
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.From = new MailAddress(cfg.SenderAddress);
                        msg.Subject = variables.FormatText(cfg.Subject);
                        msg.Body = message;
                        msg.IsBodyHtml = false;
                        msg.BodyEncoding = Encoding.Default;
                        cfg.Recipients.ForEach(n =>
                                                   {
                                                       if (n.RecipientType == RecipientType.To)
                                                       {
                                                           msg.To.Add(new MailAddress(n.Address, n.DisplayName));
                                                       }
                                                       else if (n.RecipientType == RecipientType.Cc)
                                                       {
                                                           msg.CC.Add(new MailAddress(n.Address, n.DisplayName));
                                                       }
                                                       else if (n.RecipientType == RecipientType.Bcc)
                                                       {
                                                           msg.Bcc.Add(n.Address);
                                                       }
                                                   });
                        client.Send(msg);
                    }
                }
            }
        }
    }
}
