using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ITVComponents.DataAccess.Models;

namespace PeriodicTasks.DatabaseDrivenTaskLoading.MetaData
{
    public interface IPeriodicTask
    {
        [DbColumn("Name")]
        string Name { get; set; }
        [DbColumn("Description")]
        string Description { get; set; }
        [DbColumn("Active")]
        bool Active { get; set; }
        [DbColumn("Priority")]
        int Priority { get; set; }
        [DbColumn(true)]
        string ExclusiveAreaName { get; set; }
    }
}
