using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITVComponents.DataExchange.Configuration;
using ITVComponents.Settings;
using ITVComponents.Settings.Native;
using PeriodicTasks.DataExchange.Config;

namespace PeriodicTasks.DataExchange
{
    [Serializable]
    public class DataExchangeConfig:JsonSettingsSection
    {
        /// <summary>
        /// Gets or sets a value indicating whether to use this Configuration object. If the value is set to false, the old .net Settings environment is used.
        /// </summary>
        public bool UseExtConfig { get; set; } = false;

        /// <summary>
        /// Gets a list of structure-selection configurations
        /// </summary>
        public RootConfigurationCollection StructureConfigurations { get; set; } = new RootConfigurationCollection();

        /// <summary>
        /// Gets a list of Data-Dump configurations
        /// </summary>
        public DumpConfigurationCollection DumpConfigurations { get; set; } = new DumpConfigurationCollection();

        protected override void LoadDefaults()
        {
            StructureConfigurations.Clear();
            DumpConfigurations.Clear();
            StructureConfigurations.AddRange(Helper.NativeSection.StructureConfigurations);
            DumpConfigurations.AddRange(Helper.NativeSection.DumpConfigurations);
        }

        public static class Helper
        {
            private static DataExchangeConfig Section => GetSection<DataExchangeConfig>("PeriodicTasks_DataExchange_DataExchangeConfiguration");

            private static DataExchangeSettings nativeSection;
            internal static DataExchangeSettings NativeSection => nativeSection ??= NativeSettings.GetSection<DataExchangeSettings>("ITVenture:PeriodicTasks:DataExchange");

            public static RootConfigurationCollection StructureConfigurations => Section.UseExtConfig ? Section.StructureConfigurations : NativeSection.StructureConfigurations;

            public static DumpConfigurationCollection DumpConfigurations => Section.UseExtConfig ? Section.DumpConfigurations : NativeSection.DumpConfigurations;
        }
    }
}
