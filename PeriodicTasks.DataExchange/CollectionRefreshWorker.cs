using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ITVComponents.CommandLineParser;
using ITVComponents.DataAccess;
using ITVComponents.DataAccess.Extensions;
using ITVComponents.Logging;
using ITVComponents.Plugins.SelfRegistration;
using ITVComponents.UserInterface.Collections;
using PeriodicTasks.DataExchange.Config;
using PeriodicTasks.Remote;

namespace PeriodicTasks.DataExchange
{
    public class CollectionRefreshWorker:StepWorker ,ICollectionSubscriber
    {
        /// <summary>
        /// Holds a list of Subscriptions that can be refreshed on demand
        /// </summary>
        private Dictionary<string, ICollection<dynamic>> subscriptions;

        /// <summary>
        /// the Commandline parser that is used to process the provided command
        /// </summary>
        private CommandLineParser parser = new CommandLineParser(typeof (RefreshSettings), false);

        /// <summary>
        /// Initializes a new instance of the CollectionRefreshWorker class
        /// </summary>
        /// <param name="registration">the registration callback that is used to register this object in the factory</param>
        public CollectionRefreshWorker() : base()
        {
            subscriptions = new Dictionary<string, ICollection<dynamic>>();
        }

        /// <summary>
        /// Registers a collection to the list of updateable targets of a source
        /// </summary>
        /// <param name="name">the name on which to register the collection</param>
        /// <param name="target">the collection that is registered</param>
        /// <returns>a value indicating whether the collection was successfully registered</returns>
        public bool RegisterCollection(string name, ICollection<dynamic> target)
        {
            lock (subscriptions)
            {
                bool retVal = !subscriptions.ContainsKey(name);
                if (retVal)
                {
                    subscriptions.Add(name, target);
                }

                return retVal;
            }
        }

        /// <summary>
        /// Removes a collection from the list of updateable targets of a source
        /// </summary>
        /// <param name="name">the name under which the collection was registered</param>
        /// <param name="target">the registered collection</param>
        /// <returns>a value indicating whether the collection could be successfully removed from the list of registered targets</returns>
        public bool UnregisterCollection(string name, ICollection<dynamic> target)
        {
            lock (subscriptions)
            {
                bool retVal = subscriptions.ContainsKey(name) && subscriptions[name] == target;
                if (retVal)
                {
                    subscriptions.Remove(name);
                }

                return retVal;
            }
        }

        /// <summary>
        /// Runs a step of a given task
        /// </summary>
        /// <param name="task">the task that owns the current step</param>
        /// <param name="command">the command that will be evaluated by this worker</param>
        /// <param name="values">the variables that hold results of previous steps</param>
        /// <returns>the result of the provided command</returns>
        protected override object RunTask(PeriodicTask task, string command, Dictionary<StepParameter, object> values)
        {
            RefreshSettings settings = new RefreshSettings();
            parser.Configure(command, settings);
            var bla = settings.ShowHelp;
            ICollection<dynamic> target;
            IEnumerable<DynamicResult> source;
            if (settings.ShowHelp)
            {
                task.Log(parser.PrintUsage(false,0,100), LogMessageType.Report);
                return null;
            }

            task.Log("looking up lists",LogMessageType.Report);
            lock (subscriptions)
            {
                if (!subscriptions.ContainsKey(settings.TargetCollection))
                {
                    throw new ArgumentException("Target", "The Target collection is not registered as update target");
                }

                target = subscriptions[settings.TargetCollection];
                task.Log(target.ToString(),LogMessageType.Report);
            }

            source = GetParameterValue<IEnumerable<DynamicResult>>(settings.SourceParameter, values, task);
            if (source == null)
            {
                throw new ArgumentException("Source","The Source Parameter does not contain a valid items-source");
            }

            target.Clear();
            if (target is ObservableUICollection<dynamic>)
            {
                ((ObservableUICollection<dynamic>) target).AddRange(source);
            }
            else
            {
                source.ForEach(target.Add);
            }
            task.Log(string.Format("Target contains now {0} items",target.Count),LogMessageType.Report);
            return true;
        }

        public override WorkerDescription Describe()
        {
            var desc = base.Describe();
            desc.CommandDescription = parser.PrintUsage(false, 0, 100);
            desc.Description = @"Refreshes an ICollection using an IEnumerable containing DynamicResult objects.
The name of Source and TargetObject can be specified using the Command-Arguments.";
            desc.ExpectedParameters = new[]
            {
                new ParameterDescription
                {
                    Name = "[/Source]",
                    Description = "The Source-Object from which to refresh the target collection.",
                    ExpectedTypeName = "IEnumerable<DynamicResult>",
                    Required = true
                },
                new ParameterDescription
                {
                    Name = "[/Target]",
                    Description = "The Target-Object into which the results contained in the Source are filled.",
                    ExpectedTypeName = "ICollection<DynamicResult>",
                    Required = true
                },

            };
            desc.ReturnDescription = "Returns true, if the update-process was successful";
            desc.ReturnType = "Boolean";
            return desc;
        }
    }
}
