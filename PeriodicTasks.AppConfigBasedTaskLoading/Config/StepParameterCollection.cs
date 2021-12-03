using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace PeriodicTasks.AppConfigBasedTaskLoading.Config
{
    [Serializable]
    public class StepParameterCollection : List<StepParameterDefinition>
    {
        /// <summary>
        /// Gets the configurationitem with the given name
        /// </summary>
        /// <param name="name">the name of the requested item</param>
        /// <returns>the instance of the requested configuration item</returns>
        public StepParameterDefinition this[string aname] => (from t in this where t.ParameterName == aname select t).FirstOrDefault();
    }
}
