using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ITVComponents.Settings;
using Newtonsoft.Json;

namespace PeriodicTasks.MailLogger.Config
{
    [Serializable]
    public class MailConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the MailConfiguration class
        /// </summary>
        public MailConfiguration()
        {
            Recipients = new RecipientCollection();
            Events = new EventConfigurationCollection(); 
        }

        /// <summary>
        /// The Mailserver that is used to send the log message
        /// </summary>
        public string MailServer { get; set; }

        /// <summary>
        /// The Port of the Mailserver that runs an SMTP - instance
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Indicates whether authentication is required for the server
        /// </summary>
        public bool AuthenticationRequired { get; set; }

        /// <summary>
        /// Indicates whether to use a Secure Connection
        /// </summary>
        public bool UseSSL { get; set; }

        /// <summary>
        /// The Mail-user that will be used for authentication
        /// </summary>
        public string MailUser { get; set; }

        /// <summary>
        /// The Mail-Subject for the log-mail
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// The encrypted Password of the authentication user
        /// </summary>
        [JsonConverter(typeof(JsonStringEncryptConverter))]
        public string MailPassword { get; set; }

        /// <summary>
        /// The Sender-Address
        /// </summary>
        public string SenderAddress { get; set; }

        /// <summary>
        /// A list of recipients that will get this mail
        /// </summary>
        public RecipientCollection Recipients { get; set; }

        /// <summary>
        /// a collection that holds events that need to be protocolled
        /// </summary>
        public EventConfigurationCollection Events { get; set; }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(MailServer))
            {
                return $"{MailServer} ({SenderAddress}, @{Port}, {(UseSSL?"SSL":"NOSSL")}, {Recipients.Count} recipients, {Events.Count} events)";
            }

            return base.ToString();
        }
    }
}
