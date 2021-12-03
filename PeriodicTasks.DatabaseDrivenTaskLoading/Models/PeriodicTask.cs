using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PeriodicTasks.DatabaseDrivenTaskLoading.Models	
{
	public class PeriodicTask
	{
        [Key]
	    public virtual int PeriodicTaskId { get; set; }

	    public virtual string Name { get; set; }

	    public virtual string Description { get; set; }

	    public virtual bool Active { get; set; }

	    public virtual int Priority { get; set; }

        public virtual string ExclusiveAreaName { get; set; }

        public virtual ICollection<PeriodicStep> PeriodicSteps { get; } = new HashSet<PeriodicStep>();

	    public virtual ICollection<PeriodicRun> PeriodicRuns { get; } = new HashSet<PeriodicRun>();

	    public virtual ICollection<PeriodicSchedule> PeriodicSchedules { get; } = new HashSet<PeriodicSchedule>();
	}
}
#pragma warning restore 1591
