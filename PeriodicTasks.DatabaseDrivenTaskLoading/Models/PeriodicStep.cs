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
	    public int PeriodicStepId { get; set; }

	    public int PeriodicTaskId { get; set; }

	    public string Name { get; set; }

	    public string WorkerName { get; set; }

	    public string OutputVariable { get; set; }

	    public string Command { get; set; }

	    public int StepOrder { get; set; }

        public string ExclusiveAreaName { get; set; }

        public string? TenantId { get; set; }

        [ForeignKey("PeriodicTaskId")]
	    public virtual PeriodicTask PeriodicTask { get; set; }

	    public virtual ICollection<PeriodicStepParameter> PeriodicStepParameters { get; } = new HashSet<PeriodicStepParameter>();

	    public virtual ICollection<PeriodicLog> PeriodicLogs { get; } = new HashSet<PeriodicLog>();
	}
}
