using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ITVComponents.DataAccess;

namespace PeriodicTasks.DataExchange
{
    public interface ICollectionSubscriber
    {
        /// <summary>
        /// Registers a collection to the list of updateable targets of a source
        /// </summary>
        /// <param name="name">the name on which to register the collection</param>
        /// <param name="target">the collection that is registered</param>
        /// <returns>a value indicating whether the collection was successfully registered</returns>
        bool RegisterCollection(string name, ICollection<dynamic> target);

        /// <summary>
        /// Removes a collection from the list of updateable targets of a source
        /// </summary>
        /// <param name="name">the name under which the collection was registered</param>
        /// <param name="target">the registered collection</param>
        /// <returns>a value indicating whether the collection could be successfully removed from the list of registered targets</returns>
        bool UnregisterCollection(string name, ICollection<dynamic> target);
    }
}
