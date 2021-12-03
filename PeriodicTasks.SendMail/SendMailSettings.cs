using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeriodicTasks.SendMail.Config;

namespace PeriodicTasks.SendMail
{
    public class SendMailSettings
    {
        public MailConfigurationCollection MailSettings { get; set; } = new MailConfigurationCollection();
    }
}
