using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeriodicTasks.Exchange.Config;

namespace PeriodicTasks.Exchange
{
    public class ExchangeSettings
    {
        public MailConfigCollection MailSettings { get; set; } = new MailConfigCollection();
        public AttachmentDownloadConfigCollection AttachmentSettings { get; set; } = new AttachmentDownloadConfigCollection();
    }
}
