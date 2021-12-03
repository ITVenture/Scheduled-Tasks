using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITVComponents.DataExchange.Configuration;
using PeriodicTasks.DataExchange.Config;

namespace PeriodicTasks.DataExchange
{
    public class DataExchangeSettings
    {
        public DumpConfigurationCollection DumpConfigurations { get; set; } = new DumpConfigurationCollection();
        public RootConfigurationCollection StructureConfigurations { get; set; } = new RootConfigurationCollection();
    }
}
