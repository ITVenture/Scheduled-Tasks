using System;
using System.Collections.Generic;
using System.Linq;

namespace PeriodicTasks.Exchange.Config
{
    [Serializable]
    public class MailConfigCollection : List<MailConfig>
    {
        public MailConfig this[string name] => (from t in this where t.Name == name select t).FirstOrDefault();
    }
}
