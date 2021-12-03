using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeriodicTasks.Remote
{
    [Serializable]
    public class ParameterDescription
    {
        public string Name { get; set; }

        public string ExpectedTypeName { get; set; }

        public string Description { get; set; }

        public bool Required { get; set; }
    }
}
