using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITVComponents.Settings;
using ITVComponents.Settings.Native;
using PeriodicTasks.MailLogger.Config;

namespace PeriodicTasks.MailLogger
{
    [Serializable]
    public class MailLogConfig:JsonSettingsSection
    {
        /// <summary>
        /// Gets or sets a value indicating whether to use this Configuration object. If the value is set to false, the old .net Settings environment is used.
        /// </summary>
        public bool UseExtConfig { get; set; } = false;

        public MailConfigurationCollection Schemas { get; set; } = new MailConfigurationCollection();

        protected override void LoadDefaults()
        {
            Schemas.Clear();
            Schemas.AddRange(Helper.NativeSection.Schemas);
        }

        public static class Helper
        {
            private static MailLogSettings nativeSection;
            internal static MailLogSettings NativeSection => nativeSection ??= NativeSettings.GetSection<MailLogSettings>("ITVenture:PeriodicTasks:Log2Mail");
            private static MailLogConfig Section => GetSection<MailLogConfig>("PeriodicTasks_MailLogger_MailLogConfiguration");

            public static MailConfigurationCollection Schemas => Section.UseExtConfig ? Section.Schemas : NativeSection.Schemas;
        }
    }
}
