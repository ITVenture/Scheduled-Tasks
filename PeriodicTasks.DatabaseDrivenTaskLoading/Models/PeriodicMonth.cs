using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace PeriodicTasks.DatabaseDrivenTaskLoading.Models	
{
    [Index(nameof(TenantId), IsUnique = false, Name = "IX_ScheduleMonthTenant")]
    public class PeriodicMonth
	{
        [Key]
	    public int PeriodicMonthId { get; set; }

	    public int PeriodicScheduleId { get; set; }

        public string? TenantId { get; set; }

        public bool Jan { get; set; }

        public bool Feb { get; set; }

        public bool Mar { get; set; }

        public bool Apr { get; set; }

        public bool May { get; set; }

        public bool Jun { get; set; }

        public bool Jul { get; set; }

        public bool Aug { get; set; }

        public bool Sep { get; set; }

	    public bool Oct { get; set; }

	    public bool Nov { get; set; }

	    public bool Dec { get; set; }

        [ForeignKey("PeriodicScheduleId")]
	    public virtual PeriodicSchedule PeriodicSchedule { get; set; }
	}
}
