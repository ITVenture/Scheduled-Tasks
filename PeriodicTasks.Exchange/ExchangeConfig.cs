using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITVComponents.Settings;
using ITVComponents.Settings.Native;
using PeriodicTasks.Exchange.Config;

namespace PeriodicTasks.Exchange
{
    [Serializable]
    public class ExchangeConfig:JsonSettingsSection
    {
        /// <summary>
        /// Indicates whether to use the writable configuration
        /// </summary>
        public bool UseExtConfig { get; set; } = false;

        /// <summary>
        /// Gets a list of Settings for for exchange credentials
        /// </summary>
        public MailConfigCollection MailSettings { get; set; } = new MailConfigCollection();

        /// <summary>
        /// Gets a list of settings for attachment downloading instructions
        /// </summary>
        public AttachmentDownloadConfigCollection AttachmentSettings { get; set; } = new AttachmentDownloadConfigCollection();

        /// <summary>
        /// Offers a derived class to define default-configuration-settings
        /// </summary>
        protected override void LoadDefaults()
        {
            MailSettings.Add(new MailConfig
            {
                Name = "default_POP",
                Password = "encrypt:12345",
                Port = 110,
                Server = "server.domain.intra",
                UseSsl = false,
                UserName = "user"
            });
        }

        public static class Helper
        {
            private static ExchangeConfig Section => GetSection<ExchangeConfig>("PeriodicTaskExtensions_ExchangeBinding_Settings");

            private static ExchangeSettings nativeSection;
            internal static ExchangeSettings NativeSection => nativeSection ??= NativeSettings.GetSection<ExchangeSettings>("ITVenture:PeriodicTasks:ExchangeBinding");

            public static MailConfigCollection MailSettings => Section.UseExtConfig ? Section.MailSettings : NativeSection.MailSettings;

            public static AttachmentDownloadConfigCollection AttachmentSettings => Section.UseExtConfig ? Section.AttachmentSettings : NativeSection.AttachmentSettings;
        }
    }
}
