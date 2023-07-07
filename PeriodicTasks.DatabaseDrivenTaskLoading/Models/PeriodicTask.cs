using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace PeriodicTasks.DatabaseDrivenTaskLoading.Models
{
    [Index(nameof(TenantId), IsUnique = false, Name = "IX_TaskTenant")]
    public class PeriodicTask
	{
        [Key]
	    public int PeriodicTaskId { get; set; }

	    public string Name { get; set; }

	    public string Description { get; set; }
		
	    public bool Active { get; set; }

	    public int Priority { get; set; }

        public string ExclusiveAreaName { get; set; }

        public string? TenantId { get; set; }

        public virtual ICollection<PeriodicStep> PeriodicSteps { get; } = new HashSet<PeriodicStep>();

	    public virtual ICollection<PeriodicRun> PeriodicRuns { get; } = new HashSet<PeriodicRun>();

	    public virtual ICollection<PeriodicSchedule> PeriodicSchedules { get; } = new HashSet<PeriodicSchedule>();
	}
}
#pragma warning restore 1591
