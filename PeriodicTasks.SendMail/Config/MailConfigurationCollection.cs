using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PeriodicTasks.SendMail.Config
{
    [Serializable]
    public class MailConfigurationCollection : List<MailConfiguration>
    {
        /// <summary>
        /// Gets the configurationitem with the given name
        /// </summary>
        /// <param name="configurationName">the name of the requested item</param>
        /// <returns>the instance of the requested configuration item</returns>
        public MailConfiguration this[string configurationName] => (from t in this where t.ConfigurationName == configurationName select t).First();
    }
}