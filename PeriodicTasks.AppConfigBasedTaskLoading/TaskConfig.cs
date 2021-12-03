using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITVComponents.Settings;
using ITVComponents.Settings.Native;
using PeriodicTasks.AppConfigBasedTaskLoading.Config;

namespace PeriodicTasks.AppConfigBasedTaskLoading
{
    [Serializable]
    public class TaskConfig:JsonSettingsSection
    {
        /// <summary>
        /// Synchronhizes the access to the settings for the periodic tasks
        /// </summary>
        public static object SettingsLock { get; } = new object();

        /// <summary>
        /// Gets or sets a value indicating whether to use this Configuration object. If the value is set to false, the old .net Settings environment is used.
        /// </summary>
        public bool UseExtConfig { get; set; } = false;

        /// <summary>
        /// Gets a list of defined tasks
        /// </summary>
        public TaskCollection Tasks { get; set; } = new TaskCollection();


        protected override void LoadDefaults()
        {
            Tasks.Clear();
            Tasks.AddRange(Helper.NativeSection.Tasks);
        }

        public static class Helper
        {
            private static TaskConfig Section => GetSection<TaskConfig>("PeriodicTasks_AppConfigBasedTaskLoading_TaskConfiguration");

            private static TaskSettings nativeSection;
            internal static TaskSettings NativeSection => nativeSection ??= NativeSettings.GetSection<TaskSettings>("ITVenture:PeriodicTasks:ConfigTasks");

            public static TaskCollection Tasks => Section.UseExtConfig ? Section.Tasks : NativeSection.Tasks;

            public static void Reload()
            {
                if (!Section.UseExtConfig)
                {
                    nativeSection = null;
                }
            }
        }
    }
}
