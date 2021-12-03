using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeriodicTasks.Exchange.Config
{
    [Serializable]
    public class AttachmentDownloadConfig
    {
        public string Name{get;set;}

        public string AttachmentPath { get; set; }

        public string DefaultNamingFormat { get; set; }

        public string SubjectPattern { get; set; }

        public AcceptanceBehavior DefaultSenderBehavior { get; set; } = AcceptanceBehavior.Reject;

        public AcceptanceBehavior DefaultAttachmentBehavior { get; set; } = AcceptanceBehavior.Reject;

        public bool DeleteAcceptedMessages{get; set; }

        public bool DeleteRejectedMessages { get; set; }

        public AddressPolicyCollection AddressPolicies { get; set; } = new AddressPolicyCollection();

        public AttachmentPolicyCollection AttachmentPolicies { get; set; } = new AttachmentPolicyCollection();

    }
}
