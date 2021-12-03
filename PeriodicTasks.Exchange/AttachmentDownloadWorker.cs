using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ITVComponents.CommandLineParser;
using ITVComponents.Formatting;
using ITVComponents.Security;
using ITVComponents.Settings;
using Microsoft.Exchange.WebServices.Data;
using PeriodicTasks.Exchange.Config;

namespace PeriodicTasks.Exchange
{
    public class AttachmentDownloadWorker:StepWorker
    {
        private CommandLineParser commandParser = new CommandLineParser(typeof(AttachmentWorkerArguments), false);

        public AttachmentDownloadWorker()
        {
            ConsumedSections = new[] {new JsonSectionDefinition {Name = "PeriodicTaskExtensions_ExchangeBinding_Settings", SettingsType = typeof(ExchangeConfig)}};
        }

        public override void ReadSettings()
        {
            var bla = ExchangeConfig.Helper.MailSettings;
        }

        protected override object RunTask(PeriodicTask task, string command, Dictionary<StepParameter, object> values)
        {
            AttachmentWorkerArguments args = new AttachmentWorkerArguments();
            commandParser.Configure(command, args);
            if (!args.ShowHelp)
            {
                List<Dictionary<string,object>> mailAttachments = new List<Dictionary<string, object>>();
                var mailCfg = ExchangeConfig.Helper.MailSettings[args.ExchangeConfig];
                var attachmentDownloadConfig = ExchangeConfig.Helper.AttachmentSettings[args.AttachmentConfig];
                if (mailCfg != null && attachmentDownloadConfig != null)
                {
                    task.Log(string.Format("Connecting... {0} on port {1}", mailCfg.Server, mailCfg.Port), LogMessageType.Report);
                    ExchangeService client = new ExchangeService();
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    if (mailCfg.AcceptAnyCertificate)
                    {
                        ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
                    }

                    client.Credentials = new WebCredentials(mailCfg.UserName, mailCfg.Password.Decrypt());
                    client.Url = new Uri(mailCfg.Server);
                    var itemProperty = new PropertySet(PropertySet.FirstClassProperties);
                    //itemProperty.Add(ItemSchema.Attachments);
                    var mailProperties = new PropertySet(PropertySet.FirstClassProperties);
                    mailProperties.Add(ItemSchema.Attachments);
                    itemProperty.RequestedBodyType = BodyType.Text;
                    var mailFolder = new FolderId(WellKnownFolderName.Inbox);
                    var view = new ItemView(100);
                    view.PropertySet = itemProperty;
                    var messages = client.FindItems(mailFolder, view).ConfigureAwait(false).GetAwaiter().GetResult();
                    List<ItemId> itemIds = new List<ItemId>();
                    try
                    {
                        for (int i = 0; i < messages.Items.Count; i++)
                        {
                            task.Log($"Reading Item {i + 1} of {messages.Items.Count}...", LogMessageType.Report);
                            var item = messages.Items[i];
                            EmailMessage mail = EmailMessage.Bind(client, item.Id, mailProperties).ConfigureAwait(false).GetAwaiter().GetResult();
                            if (mail != null)
                            {
                                mail.Load();
                                //Message msg = client.GetMessage(i);
                                string uid = mail.InternetMessageId;
                                //Microsoft.Exchange.WebServices.Data.item
                                string from = mail.From.Name; //.Headers.From.DisplayName;
                                string fromMail = mail.From.Address;
                                string subject = mail.Subject;
                                task.Log(string.Format("Processing Mail ({0}) from {1}. Subject: {2}", uid, from, subject), LogMessageType.Report);
                                try
                                {
                                    bool accept = true;
                                    if (!string.IsNullOrEmpty(attachmentDownloadConfig.SubjectPattern))
                                    {
                                        accept = Regex.IsMatch(subject, attachmentDownloadConfig.SubjectPattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);
                                    }

                                    accept &= CheckAddressPolicies(fromMail, attachmentDownloadConfig.AddressPolicies, attachmentDownloadConfig.DefaultSenderBehavior);
                                    if (accept)
                                    {
                                        task.Log(string.Format("Policies were checked for Mail ({0}). Mail was accepted.", uid), LogMessageType.Report);
                                        if (mail.HasAttachments)
                                        {
                                            task.Log(string.Format("Mail ({0}) has {1} Attachment(s).", uid, mail.Attachments.Count), LogMessageType.Report);
                                            foreach (var attachment in mail.Attachments)
                                            {
                                                if (attachment is FileAttachment fileAttachment)
                                                {
                                                    string resultingFileName = ProcessAttachment(fileAttachment, attachmentDownloadConfig, task);
                                                    if (!string.IsNullOrEmpty(resultingFileName))
                                                    {
                                                        mailAttachments.Add(new Dictionary<string, object>
                                                        {
                                                            {"FileName", resultingFileName},
                                                            {"Sender", from},
                                                            {"SenderAddress", fromMail},
                                                            {"Subject", subject}
                                                        });
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (accept && attachmentDownloadConfig.DeleteAcceptedMessages || !accept && attachmentDownloadConfig.DeleteRejectedMessages)
                                    {
                                        itemIds.Add(mail.Id);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    task.Log(ex.ToString(), LogMessageType.Warning);
                                }
                            }
                        }

                    }
                    finally
                    {
                        task.Log("Cleaning up...", LogMessageType.Report);
                        if (itemIds.Count != 0)
                        {
                            task.Log("Deleting Mails...", LogMessageType.Report);
                            client.DeleteItems(itemIds, DeleteMode.HardDelete, null, null);
                        }
                    }

                    return mailAttachments;
                }

                task.Log("Invalid Configuration!", LogMessageType.Error);
            }

            task.Log(commandParser.PrintUsage(false, 0, 80), LogMessageType.Report);
            return null;
        }

        private string ProcessAttachment(FileAttachment attachment, AttachmentDownloadConfig config, PeriodicTask task)
        {
            if (CheckAttachmentPolicies(attachment, config.AttachmentPolicies, config.DefaultAttachmentBehavior, out var acceptedRule))
            {
                var nameRule = !string.IsNullOrEmpty(acceptedRule?.NamingFormat) ? acceptedRule.NamingFormat : config.DefaultNamingFormat;
                task.Log(string.Format("Attachment {0} was accepted. Saving attachment using the Rule {1}...", attachment.Name, nameRule), LogMessageType.Report);
                var pth = typeof(Path);
                var tmp = new Dictionary<string, object>
                {
                    {"Name", attachment.Name},
                    {"Path", pth},
                    {"Dir", config.AttachmentPath}
                };

                string fileName = tmp.FormatText(nameRule);
                string tmpFile = fileName;
                int addition = 0;
                while(File.Exists(tmpFile))
                {
                    tmpFile = $"{fileName}.{addition++}";
                }

                fileName = tmpFile;
                attachment.Load(fileName);
                return fileName;
            }

            return null;
        }

        private bool CheckAttachmentPolicies(Attachment attachment, AttachmentPolicyCollection policies, AcceptanceBehavior defaultBehavior, out AttachmentPolicy acceptedRule)
        {
            acceptedRule = null;
            if (defaultBehavior == AcceptanceBehavior.Reject)
            {
                acceptedRule = policies.FirstOrDefault(p => CheckAttachmentPolicy(p, attachment));
                return acceptedRule != null;
            }

            return policies.Count == 0 || policies.All(p => CheckAttachmentPolicy(p, attachment));
        }

        private bool CheckAddressPolicies(string senderAddress, AddressPolicyCollection policies, AcceptanceBehavior defaultBehavior)
        {
            var differs = policies.Any(p => CheckAddressPolicy(senderAddress, p) != defaultBehavior);
            return (defaultBehavior == AcceptanceBehavior.Accept) ^ differs;
        }

        private bool CheckAttachmentPolicy(AttachmentPolicy policy, Attachment attachment)
        {
            bool retVal = policy.Behavior == AcceptanceBehavior.Accept;
            bool isOk = Regex.IsMatch(attachment.Name, policy.NameRegex, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);
            return !(retVal ^ isOk);
        }

        private AcceptanceBehavior CheckAddressPolicy(string senderAddress, AddressPolicy policy)
        {
            bool isOk = Regex.IsMatch(senderAddress, policy.AddressRegex, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);
            var nokPolicy = policy.Behavior == AcceptanceBehavior.Reject ? AcceptanceBehavior.Accept : AcceptanceBehavior.Reject;
            return isOk ? policy.Behavior : nokPolicy;
        }
    }
}
