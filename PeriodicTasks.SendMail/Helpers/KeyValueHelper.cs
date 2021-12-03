using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITVComponents.ExtendedFormatting;

namespace PeriodicTasks.SendMail.Helpers
{
    public class KeyValueHelper:IBasicKeyValueProvider
    {
        private Func<string, Type, object> source;

        public KeyValueHelper(Func<string, Type, object> source)
        {
            this.source = source;
        }

        public bool ContainsKey(string key)
        {
            return true;
        }

        public object this[string name] => source(name, typeof(object));

        public string[] Keys { get; } = Array.Empty<string>();
    }
}
