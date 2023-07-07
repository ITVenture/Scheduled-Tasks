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
	    public int PeriodicMonthdayId { get; set; }

	    public int DayNum { get; set; }

	    public int PeriodicScheduleId { get; set; }

        public string? TenantId { get; set; }

        [ForeignKey("PeriodicScheduleId")]
	    public virtual PeriodicSchedule PeriodicSchedule { get; set; }
	}
}
