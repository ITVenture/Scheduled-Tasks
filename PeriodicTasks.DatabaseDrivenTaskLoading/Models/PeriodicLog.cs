using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;

namespace PeriodicTasks.DatabaseDrivenTaskLoading.Models
{
    [Index(nameof(TenantId), IsUnique = false, Name = "IX_LogMessageTenant")]
    public class PeriodicLog
	{
        [Key]
	    public int PeriodicLogId { get; set; }

	    public int PeriodicRunId { get; set; }

	    public int? PeriodicStepId { get; set; }

	    public string Message { get; set; }

	    public int MessageType { get; set; }

	    public DateTime LogTime { get; set; }

		public string? TenantId { get; set; }

        [ForeignKey("PeriodicRunId")]
	    public virtual PeriodicRun PeriodicRun { get; set; }

        [ForeignKey("PeriodicStepId")]
	    public virtual PeriodicStep PeriodicStep { get; set; }
	}
}
