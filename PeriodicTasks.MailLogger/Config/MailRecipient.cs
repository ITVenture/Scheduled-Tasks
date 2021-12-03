using System;

namespace PeriodicTasks.MailLogger.Config
{
    [Serializable]
    public class MailRecipient
    {
        /// <summary>
        /// Gets or sets the Display - Name of the E-Mail recipient
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the E-Mail - Address of the Recipient
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the Kind of Recipient that is configured with this item (To/CC/BCC)
        /// </summary>
        public RecipientType RecipientType { get; set; }
        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Address))
            {
                return $"{RecipientType} {Address} ({DisplayName})";
            }

            return base.ToString();
        }
    }

    public enum RecipientType
    {
        /// <summary>
        /// Normal Recipient
        /// </summary>
        To,

        /// <summary>
        /// Copy-Recipient
        /// </summary>
        Cc,

        /// <summary>
        /// Undisclosed - Copy Recipient
        /// </summary>
        Bcc
    }
}
