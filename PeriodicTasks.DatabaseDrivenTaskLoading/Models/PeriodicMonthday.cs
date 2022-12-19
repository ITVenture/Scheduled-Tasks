using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace PeriodicTasks.DatabaseDrivenTaskLoading.Models
{
    [Index(nameof(TenantId), IsUnique = false, Name = "IX_ScheduleMonthDayTenant")]
    public class PeriodicMonthday
	{
        [Key]
	    public virtual int PeriodicMonthdayId { get; set; }

	    public virtual int DayNum { get; set; }

	    public virtual int PeriodicScheduleId { get; set; }

        public virtual string? TenantId { get; set; }

        [ForeignKey("PeriodicScheduleId")]
	    public virtual PeriodicSchedule PeriodicSchedule { get; set; }
	}
}
