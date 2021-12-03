using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeriodicTasks.SendMail.Config
{
    [Serializable]
    public class MailAttachment
    {
        /// <summary>
        /// Gets or sets the Source file of the Mail Attachment
        /// </summary>
        public string SourceFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore a missing file. When set to false, an exception is thrown when a file is not provided
        /// </summary>
        public bool IgnoreWhenMissing { get; set; }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(SourceFile))
            {
                return $"{SourceFile} ({(IgnoreWhenMissing?"Optional":"Required")})";
            }

            return base.ToString();
        }
    }
}
