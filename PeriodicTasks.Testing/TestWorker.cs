using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ITVComponents.Plugins.SelfRegistration;
using ITVComponents.Scripting.CScript.Core;
using ITVComponents.Scripting.CScript.Helpers;

namespace PeriodicTasks.Testing
{
    public class TestWorker:StepWorker
    {
        public TestWorker() : base()
        {
            
        }

        protected override object RunTask(PeriodicTask task, string command, Dictionary<StepParameter, object> values)
        {
            Dictionary<string, object> vars = new Dictionary<string, object>();
            foreach (KeyValuePair<StepParameter, object> var in values)
            {
                vars.Add(var.Key.ParameterName, var.Value);
            }

            object retVal = ExpressionParser.Parse(command, vars, a => { DefaultCallbacks.PrepareDefaultCallbacks(a.Scope, a.ReplSession); });
            Console.WriteLine("{0} = {1}", command, retVal);
            return retVal;
        }
    }
}
