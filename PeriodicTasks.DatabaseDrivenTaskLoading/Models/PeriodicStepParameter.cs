using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace PeriodicTasks.DatabaseDrivenTaskLoading.Models
{
    [Index(nameof(TenantId), IsUnique = false, Name = "IX_StepParamTenant")]
    public class PeriodicStepParameter
	{
        [Key]
	    public virtual int PeriodicStepParameterId { get; set; }

	    public virtual int PeriodicStepId { get; set; }

	    public virtual string ParameterName { get; set; }

	    public virtual string ParameterType { get; set; }

	    public virtual string Value { get; set; }

	    public virtual string Settings { get; set; }

        public virtual string? TenantId { get; set; }

        [ForeignKey("PeriodicStepId")]
	    public virtual PeriodicStep PeriodicStep { get; set; }
	}
}
