using System;

namespace PeriodicTasks.Exchange.Config
{
    [Serializable]
    public class AddressPolicy
    {
        public string AddressRegex{get;set;}

        public AcceptanceBehavior Behavior { get; set; }
    }
}
