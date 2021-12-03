using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ITVComponents.Settings;
using Newtonsoft.Json;

namespace PeriodicTasks.SendMail.Config
{
    [Serializable]
    public class MailConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the MailConfiguration class
        /// </summary>
        public MailConfiguration()
        {
            Attachments = new AttachmentCollection();
            Recipients = new RecipientCollection();
            MailServerPort = 25;
        }

        /// <summary>
        /// Gets or sets the Mailserver that was used to send the mail message
        /// </summary>
        public string MailServer { get; set; }

        /// <summary>
        /// Gets or sets the Server port for mailing
        /// </summary>
        public int MailServerPort { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use SSL
        /// </summary>
        public bool UseSsl { get; set; }

        /// <summary>
        /// Gets or sets the ConfigurationName of this MailConfiguration item
        /// </summary>
        public string ConfigurationName { get; set; }

        /// <summary>
        /// Gets or sets the name of the variable that is expected to hold a list of mails to send. If it is not set, only one mail is sent.
        /// </summary>
        public string ListSourceVariable { get; set; }

        /// <summary>
        /// Gets or sets the SenderAddress of this MailConfiguration item
        /// </summary>
        public string SenderAddress { get; set; }

        /// <summary>
        /// The Displayname of the Sender-Address
        /// </summary>
        public string SenderName { get; set; }

        /// <summary>
        /// The Reply-To - Address of a mail-message
        /// </summary>
        public string ReplyAddr { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Authentication is required in order to Send an E-Mail
        /// </summary>
        public bool AuthenticationRequired { get; set; }

        /// <summary>
        /// Gets or sets the UserName of the Authentication
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the Encrypted Password for the configured user
        /// </summary>
        [JsonConverter(typeof(JsonStringEncryptConverter))]
        public string Password { get; set; }

        /// <summary>
        /// Gets a list of Attachments that must be added to the sent e-mail
        /// </summary>
        public AttachmentCollection Attachments { get; set; }

        /// <summary>
        /// Gets a list of Recipients that must receive the e-mail
        /// </summary>
        public RecipientCollection Recipients { get; set; }

        /// <summary>
        /// Gets or sets the Subject of the Mail message
        /// </summary>
        public string MailSubject { get; set; }

        /// <summary>
        /// Gets or sets the TextBody of the Mail-Message
        /// </summary>
        public string MailTextBody { get; set; }

        /// <summary>
        /// Define a Parameter that must have the value true to send this message
        /// </summary>
        public string MailWhen { get; set; }

        /// <summary>
        /// Indicates whether to send a mail only when at least one attachment was successfully added
        /// </summary>
        public bool SendOnlyWithAttachments { get; set; }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(ConfigurationName))
            {
                return
                    $"{ConfigurationName} (Mode: {(!string.IsNullOrEmpty(ListSourceVariable) ? "Multi" : "Single")}, {SenderAddress}, @{MailServer}:{MailServerPort}, {Recipients.Count} recipients, {Attachments.Count} attachments)";
            }

            return base.ToString();
        }
    }
}
