using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeriodicTasks
{
    public class PlannedTermination
    {
        private static readonly PlannedTermination value = new PlannedTermination();

        public static PlannedTermination Value { get { return value; } }
    }
}
