using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PeriodicTasks.DatabaseDrivenTaskLoading.Models	
{
	public class PeriodicLog
	{
        [Key]
	    public virtual int PeriodicLogId { get; set; }

	    public virtual int PeriodicRunId { get; set; }

	    public virtual int? PeriodicStepId { get; set; }

	    public virtual string Message { get; set; }

	    public virtual int MessageType { get; set; }

	    public virtual DateTime LogTime { get; set; }

        [ForeignKey("PeriodicRunId")]
	    public virtual PeriodicRun PeriodicRun { get; set; }

        [ForeignKey("PeriodicStepId")]
	    public virtual PeriodicStep PeriodicStep { get; set; }
	}
}
