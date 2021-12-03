using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeriodicTasks.AppConfigBasedTaskLoading.Config;

namespace PeriodicTasks.AppConfigBasedTaskLoading
{
    public class TaskSettings
    {
        public TaskCollection Tasks { get; set; } = new TaskCollection();
    }
}
