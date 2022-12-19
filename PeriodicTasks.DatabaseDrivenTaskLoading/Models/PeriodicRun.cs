using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace PeriodicTasks.DatabaseDrivenTaskLoading.Models
{
    [Index(nameof(TenantId), IsUnique = false, Name = "IX_RunTenant")]
    public class PeriodicRun
	{
        [Key]
	    public virtual int PeriodicRunId { get; set; }

	    public virtual int PeriodicTaskId { get; set; }

	    public virtual DateTime StartTime { get; set; }

	    public virtual DateTime? EndTime { get; set; }

        public virtual string? TenantId { get; set; }

        [ForeignKey("PeriodicTaskId")]
	    public virtual PeriodicTask PeriodicTask { get; set; }

	    public virtual ICollection<PeriodicLog> PeriodicLogs { get; } = new HashSet<PeriodicLog>();
	}
}
