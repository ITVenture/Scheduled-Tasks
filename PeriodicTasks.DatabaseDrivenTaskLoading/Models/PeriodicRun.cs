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
	    public int PeriodicRunId { get; set; }

	    public int PeriodicTaskId { get; set; }

	    public DateTime StartTime { get; set; }

	    public DateTime? EndTime { get; set; }

        public string? TenantId { get; set; }

        [ForeignKey("PeriodicTaskId")]
	    public virtual PeriodicTask PeriodicTask { get; set; }

	    public virtual ICollection<PeriodicLog> PeriodicLogs { get; } = new HashSet<PeriodicLog>();
	}
}
