using System;
using ITVComponents.Settings;
using ITVComponents.Settings.Native;
using PeriodicTasks.SendMail.Config;

namespace PeriodicTasks.SendMail
{
    [Serializable]
    public class SendMailConfig:JsonSettingsSection
    {
        /// <summary>
        /// Gets or sets a value indicating whether to use this Configuration object. If the value is set to false, the old .net Settings environment is used.
        /// </summary>
        public bool UseExtConfig { get; set; } = false;

        public MailConfigurationCollection MailSettings { get; set; } = new MailConfigurationCollection();

        protected override void LoadDefaults()
        {
            MailSettings.Clear();
            MailSettings.AddRange(Helper.NativeSection.MailSettings);
        }

        public static class Helper
        {
            private static SendMailSettings nativeSection;
            internal static SendMailSettings NativeSection => nativeSection ??= NativeSettings.GetSection<SendMailSettings>("ITVenture:PeriodicTasks:SendMail");
            private static SendMailConfig Section => GetSection<SendMailConfig>("PeriodicTasks_SendMail_SendMailConfiguration");

            public static MailConfigurationCollection MailSettings => Section.UseExtConfig ? Section.MailSettings : NativeSection.MailSettings;
        }
    }
}
