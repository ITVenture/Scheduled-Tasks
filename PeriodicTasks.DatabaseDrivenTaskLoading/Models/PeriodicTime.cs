using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace PeriodicTasks.DatabaseDrivenTaskLoading.Models
{
    [Index(nameof(TenantId), IsUnique = false, Name = "IX_ScheduleTimeTenant")]
    public class PeriodicTime
	{
        [Key]
	    public virtual int PeriodicTimeId { get; set; }

	    public virtual string Time { get; set; }

	    public virtual int PeriodicScheduleId { get; set; }

        public virtual string? TenantId { get; set; }

        [ForeignKey("PeriodicScheduleId")]
	    public virtual PeriodicSchedule PeriodicSchedule { get; set; }
	}
}
