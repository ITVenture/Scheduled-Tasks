using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeriodicTasks.MailLogger.Config;

namespace PeriodicTasks.MailLogger
{
    public class MailLogSettings
    {
        public MailConfigurationCollection Schemas { get; set; } = new MailConfigurationCollection();
    }
}
