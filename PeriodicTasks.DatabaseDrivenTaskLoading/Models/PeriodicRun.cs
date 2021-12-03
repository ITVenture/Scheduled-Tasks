using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PeriodicTasks.DatabaseDrivenTaskLoading.Models	
{
	public class PeriodicRun
	{
        [Key]
	    public virtual int PeriodicRunId { get; set; }

	    public virtual int PeriodicTaskId { get; set; }

	    public virtual DateTime StartTime { get; set; }

	    public virtual DateTime? EndTime { get; set; }

        [ForeignKey("PeriodicTaskId")]
	    public virtual PeriodicTask PeriodicTask { get; set; }

	    public virtual ICollection<PeriodicLog> PeriodicLogs { get; } = new HashSet<PeriodicLog>();
	}
}
