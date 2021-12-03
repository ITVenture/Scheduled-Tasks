using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeriodicTasks.Exchange.Config
{

    [Serializable]
    public class AttachmentPolicy
    {
        public string NameRegex{get;set;}

        public AcceptanceBehavior Behavior { get; set; }

        public string NamingFormat { get; set; }
    }
}
