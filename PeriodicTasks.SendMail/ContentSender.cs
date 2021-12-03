﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using ITVComponents.DataAccess;
using ITVComponents.ExtendedFormatting;
using ITVComponents.Formatting;
using ITVComponents.Plugins.SelfRegistration;
using ITVComponents.Settings;
using ITVComponents.Security;
using PeriodicTasks.Remote;
using PeriodicTasks.SendMail.Config;
using PeriodicTasks.SendMail.Helpers;
using AttachmentCollection = PeriodicTasks.SendMail.Config.AttachmentCollection;

namespace PeriodicTasks.SendMail
{
    public class ContentSender:StepWorker
    {
        /// <summary>
        /// Initializes a new instance of the ContentSender class
        /// </summary>
        /// <param name="registration">the registration callback that is used by the base class</param>
        public ContentSender() : base()
        {
            ConsumedSections = new[] {new JsonSectionDefinition {Name = "PeriodicTasks_SendMail_SendMailConfiguration", SettingsType = typeof(SendMailConfig)}};
        }

        /// <summary>
        /// Runs a step of a given task
        /// </summary>
        /// <param name="task">the task that owns the current step</param>
        /// <param name="command">the command that will be evaluated by this worker</param>
        /// <param name="values">the variables that hold results of previous steps</param>
        /// <returns>the result of the provided command</returns>
        protected override object RunTask(PeriodicTask task, string command, Dictionary<StepParameter, object> values)
        {
            MailConfiguration settings = SendMailConfig.Helper.MailSettings[command];
            bool retVal = false;
            if (settings != null)
            {
                using (SmtpClient client = new SmtpClient(settings.MailServer, settings.MailServerPort))
                {
                    client.EnableSsl = settings.UseSsl;
                    if (settings.UseSsl)
                    {
                        System.Net.ServicePointManager.ServerCertificateValidationCallback =
                            (sender, certificate, chain, errors) => true;
                    }
                    if (settings.AuthenticationRequired)
                    {
                        client.UseDefaultCredentials = false;
                        client.Credentials = new NetworkCredential(settings.UserName,
                                                                   settings.Password.Decrypt());
                    }

                    if (string.IsNullOrEmpty(settings.ListSourceVariable))
                    {
                        retVal = SendMessage(task, settings, (n, t) => GetParameterValue(n, values, task, t), client);
                    }
                    else
                    {
                        var li = GetParameterValue<IList<IDictionary<string, object>>>(settings.ListSourceVariable,
                            values, task);
                        foreach (var item in li)
                        {
                            retVal |= SendMessage(task, settings, (n, t) => item[n], client);
                        }
                    }
                }
            }

            return retVal;
        }

        public override WorkerDescription Describe()
        {
            var retVal = base.Describe();
            retVal.Description=
                "Sends content that was generated by a previous step using a specific e-mail configuration";
            retVal.Remarks =
                string.Format(@"Provide a boolean variable you want to check for the 'MailWhen' condition as Step-Parameter.

Recipients in the Configuration are literals by default, unless they start with a colon (:). 
In this case, a Step-Parameter with the given name is expected.
Supported Types are:
string         : the plain e-mail - address
string[]       : a list of plain e-mail - addresses
DynamicResult  : a DynamicResult containing the fields 'DisplayName' and 'Address'
DynamicResult[]: a list of DynamicResult objects containing the fields 'DisplayName' and 'Address'

File-Attachments in the Configuration are literals by default, unless they start with a colon (:).
In this case, a Step-Parameter with the given name is expected.
Supported Types are:
string         : the plain file-name
string[]       : a list of plain file-names
DynamicResult  : a DynamicResult containing the field 'SourceFile'
DynamicResult[]: a list of DynamicResult objects containing the field 'SourceFile'

The Mail-Body and Subject in the Configuration are literals by default, unless they start with a colon (:).
In this Case, you can use the Features of the TextFormat class in ITVComponents.Formatting.dll
each Object you want to access must be provided as a Step-Parameter.

If your configuration uses the ListSourceVariable setting, the above applies to every entry in the list, otherwise only to the job-variables.

available configurations:
{0}", DumpConfigurations());
            retVal.CommandDescription = "The name of the Profile you want to use for sending the content";
            retVal.ReturnType = "boolean";
            retVal.ReturnDescription = "A value indicating whether the mail was sent";
            return retVal;
        }

        /// <summary>
        /// Dumps all Configurations available in the Config file
        /// </summary>
        /// <returns>a string representing all mail-configurations</returns>
        private string DumpConfigurations()
        {
            return string.Join("\r\n\r\n", (from t in SendMailConfig.Helper.MailSettings
                                     select string.Format(@"{0}:
Server                 : {1}:{2}
SSL                    : {3}
Sender                 : {4}
Authentication required: {6}
Username               : {5}
Send-Condition         : {7}
Mail-Subject           : {8}
Mail-Body              : {9}
Multi-MailVariable     : {12}
Recipients:
{10}
Attachments:
{11}", t.ConfigurationName, t.MailServer, t.MailServerPort, t.UseSsl, t.SenderAddress, t.UserName,
                                                          t.AuthenticationRequired, t.MailWhen, t.MailSubject,
                                                          t.MailTextBody, DumpRecipients(t.Recipients),
                                                          DumpAttachments(t.Attachments), t.ListSourceVariable)));
        }

        /// <summary>
        /// Dumps all recipients for a specific mail-confioguration
        /// </summary>
        /// <param name="recipients">the configured recipients</param>
        /// <returns>a string representing all recipients for a specific mail</returns>
        private string DumpRecipients(IEnumerable<MailRecipient> recipients)
        {
            return string.Join("\r\n",
                        (from t in recipients
                         select string.Format("{0}: {1}({2})", t.RecipientType, t.DisplayName, t.Address)));
        }

        /// <summary>
        /// Dumps all attachments in a mail-configuration
        /// </summary>
        /// <param name="attachments">a list of configure mail-attachments</param>
        /// <returns>a string representing all attachments in the configuration</returns>
        private string DumpAttachments(IEnumerable<MailAttachment> attachments)
        {
            return string.Join("\r\n", (from t in attachments select t.SourceFile));
        }

        /// <summary>
        /// Formats the mail body if required
        /// </summary>
        /// <param name="mailTextBody">the message body that was passed with the configuration</param>
        /// <param name="values">the values provided from previous steps</param>
        /// <returns>the formatted (if required) mailbody</returns>
        private string FormatBody(string mailTextBody, Func<string, Type, object> values)
        {
            if (mailTextBody == null || !mailTextBody.StartsWith(":"))
            {
                return mailTextBody;
            }

            /*Dictionary<string, object> cfg = new Dictionary<string, object>();
            foreach (var tmp in values)
            {
                cfg.Add(tmp.Key.ParameterName, tmp.Value);
            }*/
            var cfg = new KeyValueHelper(values);

            return cfg.FormatText(mailTextBody.Substring(1));
        }

        /// <summary>
        /// Checks a condition for sending a mail
        /// </summary>
        /// <param name="booleanVar">the boolean variable that needs to be checked in order to send a message</param>
        /// <param name="variables">variables that were provided from previous steps</param>
        /// <param name="task">the task that is currently running</param>
        /// <returns>a value indicating whether the condition for sending a mail is true</returns>
        private bool CheckWhen(string booleanVar, Func<string, Type, object> variables, PeriodicTask task)
        {
            try
            {
                return
                    (bool)variables(booleanVar, typeof(bool)); //GetParameterValue<bool>(booleanVar, variables, task);
            }
            catch (Exception ex)
            {
                task.Log(
                    string.Format(
                        @"An error occurred checking the condition for sending a mail. Mail is now sent anyway. 
Error: {0}",
                        ex), LogMessageType.Warning);
            }

            return true;
        }

        /// <summary>
        /// Adds attachments to the provided mail-message before it is sent
        /// </summary>
        /// <param name="task">the task providing additional arguments for e-mail attachments</param>
        /// <param name="mailMessage">the message that is about to be sent</param>
        /// <param name="mailConfig">the configuration for the provided mail</param>
        /// <param name="values">Additional arguments that are provided by previous steps of the current task</param>
        private void SetMailAttachments(PeriodicTask task, MailMessage mailMessage, MailConfiguration mailConfig, Func<string, Type, object> values, out bool attachmentsFound)
        {
            attachmentsFound = false;
            if (mailConfig.Attachments != null)
            {
                foreach (MailAttachment att in mailConfig.Attachments)
                {
                    if (att.SourceFile.StartsWith(":"))
                    {
                        object tmp = values(att.SourceFile.Substring(1), typeof(object));//GetParameterValue<object>(att.SourceFile.Substring(1), values, task);
                        if (tmp == null)
                        {
                            if (att.IgnoreWhenMissing)
                            {
                                continue;
                            }

                            throw new ArgumentException(string.Format("Argument {0} is missing",
                                                                      att.SourceFile.Substring(1)));
                        }

                        if (tmp is IEnumerable<DynamicResult>)
                        {
                            foreach (DynamicResult result in (IEnumerable<DynamicResult>) tmp)
                            {
                                AddAttachment(mailMessage, result["SourceFile"]);
                            }
                        }
                        else if (tmp is DynamicResult)
                        {
                            DynamicResult result = (DynamicResult) tmp;
                            AddAttachment(mailMessage, result["SourceFile"]);
                        }
                        else if (tmp is IEnumerable<string>)
                        {
                            foreach (string result in (IEnumerable<string>) tmp)
                            {
                                AddAttachment(mailMessage, result);
                            }
                        }
                        else if (tmp is string)
                        {
                            string result = (string) tmp;
                            AddAttachment(mailMessage, result);
                        }
                        else
                        {
                            throw new ArgumentException(string.Format(
                                "Argument {0} has an unexpected type: {1}", att.SourceFile.Substring(1),
                                tmp.GetType().FullName));
                        }
                    }
                    else
                    {
                        AddAttachment(mailMessage, att.SourceFile);
                    }

                    attachmentsFound = true;
                }
            }
        }

        /// <summary>
        /// Adds Mail Recipients based on the provided configuration to the email before it is sent
        /// </summary>
        /// <param name="task">the current task providing additional arguments that may be used by this method</param>
        /// <param name="mailMessage">the mail message that is about to be sent</param>
        /// <param name="mailConfig">the mail-configuration on which the senders of the message are built</param>
        /// <param name="values">the values that are provided by previous steps of the current executed task</param>
        private void SetMailRecipients(PeriodicTask task, MailMessage mailMessage, MailConfiguration mailConfig, Func<string, Type, object> values)
        {
            foreach (MailRecipient rec in mailConfig.Recipients)
            {
                if (rec.Address.StartsWith(":"))
                {
                    object tmp = values(rec.Address.Substring(1), typeof(object));//GetParameterValue<object>(rec.Address.Substring(1), values, task);
                    if (tmp == null)
                    {
                        throw new ArgumentException(string.Format("Missing Argument: {0}",
                                                                  rec.Address.Substring(1)));
                    }

                    if (tmp is IEnumerable<DynamicResult>)
                    {
                        foreach (DynamicResult result in ((IEnumerable<DynamicResult>) tmp))
                        {
                            AddRecipient(mailMessage, result["Address"], result["DisplayName"], rec.RecipientType);
                        }
                    }
                    else if (tmp is DynamicResult)
                    {
                        DynamicResult result = (DynamicResult) tmp;
                        AddRecipient(mailMessage, result["Address"], result["DisplayName"], rec.RecipientType);
                    }
                    else if (tmp is IEnumerable<string>)
                    {
                        foreach (string result in (IEnumerable<string>) tmp)
                        {
                            AddRecipient(mailMessage, result, "", rec.RecipientType);
                        }
                    }
                    else if (tmp is string)
                    {
                        string result = (string) tmp;
                        AddRecipient(mailMessage, result, rec.DisplayName, rec.RecipientType);
                    }
                    else
                    {
                        throw new ArgumentException(
                            string.Format("Parameter {0} has an unexpected Type: {1}",
                                          rec.Address.Substring(1), tmp.GetType().FullName));
                    }
                }
                else
                {
                    AddRecipient(mailMessage, rec.Address, rec.DisplayName, rec.RecipientType);
                }
            }
        }

        /// <summary>
        /// Adds an attachment to the list of attachments of the provided mail message
        /// </summary>
        /// <param name="mailMessage">the mail message to which to add an attachment</param>
        /// <param name="sourceFile">the source file of the attachment</param>
        private void AddAttachment(MailMessage mailMessage, string sourceFile)
        {
            Attachment attachment = new Attachment(sourceFile);
            mailMessage.Attachments.Add(attachment);
        }

        /// <summary>
        /// Adds an address to the list of recipients of the provided mail message
        /// </summary>
        /// <param name="mailMessage">the mailmessage that is currently being built</param>
        /// <param name="address">the target e-mail address that must be added</param>
        /// <param name="displayName">the displayname of the given address</param>
        /// <param name="recipientType">the recipient type of the address</param>
        private void AddRecipient(MailMessage mailMessage, string address, string displayName, RecipientType recipientType)
        {
            MailAddress addr = new MailAddress(address, displayName);
            switch (recipientType)
            {
                case RecipientType.To:
                    {
                        mailMessage.To.Add(addr);
                        break;
                    }
                case RecipientType.Cc:
                    {
                        mailMessage.CC.Add(addr);
                        break;
                    }
                case RecipientType.Bcc:
                    {
                        mailMessage.Bcc.Add(addr);
                        break;
                    }
            }
        }

        private bool SendMessage(PeriodicTask task, MailConfiguration settings, Func<string, Type, object> values, SmtpClient client)
        {
            bool retVal = false;
            using (MailMessage msg = new MailMessage())
            {
                if (settings.MailWhen == null || CheckWhen(settings.MailWhen, values, task))
                {
                    msg.From = new MailAddress(settings.SenderAddress, settings.SenderName);
                    if (!string.IsNullOrEmpty(settings.ReplyAddr))
                    {
                        msg.ReplyToList.Add(new MailAddress(settings.ReplyAddr));
                    }

                    msg.Subject = FormatBody(settings.MailSubject, values);
                    msg.Body = FormatBody(settings.MailTextBody, values);
                    msg.IsBodyHtml = false;
                    msg.BodyEncoding = Encoding.Default;
                    SetMailRecipients(task, msg, settings, values);
                    bool hasAttachments;
                    SetMailAttachments(task, msg, settings, values, out hasAttachments);
                    if (hasAttachments || !settings.SendOnlyWithAttachments)
                    {
                        client.Send(msg);
                    }

                    retVal = true;
                }
            }

            return retVal;
        }
    }
}
