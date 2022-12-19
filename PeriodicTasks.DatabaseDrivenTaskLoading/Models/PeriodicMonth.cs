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
	    public virtual int PeriodicMonthId { get; set; }

	    public virtual int PeriodicScheduleId { get; set; }

        public virtual string? TenantId { get; set; }

        public virtual bool Jan { get; set; }

        public virtual bool Feb { get; set; }

        public virtual bool Mar { get; set; }

        public virtual bool Apr { get; set; }

        public virtual bool May { get; set; }

        public virtual bool Jun { get; set; }

        public virtual bool Jul { get; set; }

        public virtual bool Aug { get; set; }

        public virtual bool Sep { get; set; }

	    public virtual bool Oct { get; set; }

	    public virtual bool Nov { get; set; }

	    public virtual bool Dec { get; set; }

        [ForeignKey("PeriodicScheduleId")]
	    public virtual PeriodicSchedule PeriodicSchedule { get; set; }
	}
}
