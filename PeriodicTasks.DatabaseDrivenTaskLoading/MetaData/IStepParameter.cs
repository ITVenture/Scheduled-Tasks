using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ITVComponents.DataAccess.Models;

namespace PeriodicTasks.DatabaseDrivenTaskLoading.MetaData
{
    public interface IStepParameter
    {
        [DbColumn("ParameterName")]
        string ParameterName { get; set; }
        [DbColumn("ParameterType")]
        ParameterType ParameterType { get; set; }
        [DbColumn("Value")]
        string Value { get; set; }
        [DbColumn("Settings")]
        string ParameterSettings { get; set; }
    }
}
