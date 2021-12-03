using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PeriodicTasks.DatabaseDrivenTaskLoading.Models	
{
	public class PeriodicSchedule
	{
        [Key]
	    public virtual int PeriodicScheduleId { get; set; }

	    public virtual string Name { get; set; }

	    public virtual string Period { get; set; }

	    public virtual DateTime FirstDate { get; set; }

	    public virtual int PeriodicTaskId { get; set; }

	    public virtual int Occurrence { get; set; }

	    public virtual int Mod { get; set; }

        public virtual bool OnServiceStart { get; set; }

        public virtual string SchedulerName { get; set; }

        [ForeignKey("PeriodicTaskId")]
	    public virtual PeriodicTask PeriodicTask { get; set; }

	    public virtual ICollection<PeriodicWeekday> PeriodicWeekdays { get; } = new HashSet<PeriodicWeekday>();

	    public virtual ICollection<PeriodicTime> PeriodicTimes { get; } = new HashSet<PeriodicTime>();

	    public virtual ICollection<PeriodicMonth> PeriodicMonths { get; } = new HashSet<PeriodicMonth>();

	    public virtual ICollection<PeriodicMonthday> PeriodicMonthdays { get; } = new HashSet<PeriodicMonthday>();
	}
}
