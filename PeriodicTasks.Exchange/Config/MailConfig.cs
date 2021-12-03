using System;
using ITVComponents.Settings;
using Newtonsoft.Json;

namespace PeriodicTasks.Exchange.Config
{
    [Serializable]
    public class MailConfig
    {
        public string Name { get; set; }

        public string Server { get; set; }

        public int Port { get; set; }

        public string UserName { get; set; }

        [JsonConverter(typeof(JsonStringEncryptConverter))]
        public string Password { get; set; }

        public bool UseSsl { get; set; }

        public bool AcceptAnyCertificate { get;set; }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                return $"{Name} (@{Server}:{Port}, {UserName}, {(UseSsl?"SSL":"NOSSL")})";
            }

            return base.ToString();
        }
    }
}
