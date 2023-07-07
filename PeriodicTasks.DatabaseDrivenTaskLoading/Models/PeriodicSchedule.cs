using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace PeriodicTasks.DatabaseDrivenTaskLoading.Models
{
    [Index(nameof(TenantId), IsUnique = false, Name = "IX_ScheduleTenant")]
    public class PeriodicSchedule
	{
        [Key]
	    public int PeriodicScheduleId { get; set; }

	    public string Name { get; set; }

	    public string Period { get; set; }

	    public DateTime FirstDate { get; set; }

	    public int PeriodicTaskId { get; set; }

	    public int Occurrence { get; set; }

	    public int Mod { get; set; }

        public bool OnServiceStart { get; set; }

        public string SchedulerName { get; set; }

        public string? TenantId { get; set; }

        [ForeignKey("PeriodicTaskId")]
	    public virtual PeriodicTask PeriodicTask { get; set; }

	    public virtual ICollection<PeriodicWeekday> PeriodicWeekdays { get; } = new HashSet<PeriodicWeekday>();

	    public virtual ICollection<PeriodicTime> PeriodicTimes { get; } = new HashSet<PeriodicTime>();

	    public virtual ICollection<PeriodicMonth> PeriodicMonths { get; } = new HashSet<PeriodicMonth>();

	    public virtual ICollection<PeriodicMonthday> PeriodicMonthdays { get; } = new HashSet<PeriodicMonthday>();
	}
}
