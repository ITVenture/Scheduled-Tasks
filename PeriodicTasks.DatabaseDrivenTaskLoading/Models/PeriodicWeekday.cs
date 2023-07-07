using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace PeriodicTasks.DatabaseDrivenTaskLoading.Models
{
    [Index(nameof(TenantId), IsUnique = false, Name = "IX_ScheduleWeekDayTenant")]
    public class PeriodicWeekday
	{
        [Key]
        public int PeriodicWeekdayId { get; set; }

        public int PeriodicScheduleId { get; set; }

	    public bool Wednesday { get; set; }

	    public bool Tuesday { get; set; }

	    public bool Thursday { get; set; }

	    public bool Sunday { get; set; }

	    public bool Saturday { get; set; }

	    public bool Monday { get; set; }

	    public bool Friday { get; set; }

        public string? TenantId { get; set; }

        [ForeignKey("PeriodicScheduleId")]
	    public virtual PeriodicSchedule PeriodicSchedule { get; set; }
	}
}
