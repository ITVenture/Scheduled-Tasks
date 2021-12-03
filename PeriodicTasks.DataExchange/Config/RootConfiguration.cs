using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ITVComponents.DataExchange.Configuration;

namespace PeriodicTasks.DataExchange.Config
{
    [Serializable]
    public class RootConfiguration
    {

        public string Name { get; set; }

        public QueryConfigurationCollection Configuration { get; set; } = new QueryConfigurationCollection(); 

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                return $"{Name} ({Configuration.Count} queries)";
            }

            return base.ToString();
        }
    }
}
