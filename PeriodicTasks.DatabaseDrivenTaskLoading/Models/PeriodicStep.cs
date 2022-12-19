using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace PeriodicTasks.DatabaseDrivenTaskLoading.Models
{
    [Index(nameof(TenantId), IsUnique = false, Name = "IX_StepTenant")]
    public class PeriodicStep
	{
        [Key]
	    public virtual int PeriodicStepId { get; set; }

	    public virtual int PeriodicTaskId { get; set; }

	    public virtual string Name { get; set; }

	    public virtual string WorkerName { get; set; }

	    public virtual string OutputVariable { get; set; }

	    public virtual string Command { get; set; }

	    public virtual int StepOrder { get; set; }

        public virtual string ExclusiveAreaName { get; set; }

        public virtual string? TenantId { get; set; }

        [ForeignKey("PeriodicTaskId")]
	    public virtual PeriodicTask PeriodicTask { get; set; }

	    public virtual ICollection<PeriodicStepParameter> PeriodicStepParameters { get; } = new HashSet<PeriodicStepParameter>();

	    public virtual ICollection<PeriodicLog> PeriodicLogs { get; } = new HashSet<PeriodicLog>();
	}
}
