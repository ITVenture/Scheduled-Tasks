using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PeriodicTasks.DataExchange.Config
{
    [Serializable]
    public class RootConfigurationCollection: List<RootConfiguration>
    {
        /// <summary>
        /// Gets the value of a query with the specified name
        /// </summary>
        /// <param name="name">the name of the requested item</param>
        /// <returns>the  requeted item or null if it does not exist</returns>
        public RootConfiguration this[string name] => this.FirstOrDefault(n => name.Equals(n.Name, StringComparison.OrdinalIgnoreCase));
    }
}
