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
	    public int PeriodicStepParameterId { get; set; }

	    public int PeriodicStepId { get; set; }

	    public string ParameterName { get; set; }

	    public string ParameterType { get; set; }

	    public string Value { get; set; }

	    public string Settings { get; set; }

        public string? TenantId { get; set; }

        [ForeignKey("PeriodicStepId")]
	    public virtual PeriodicStep PeriodicStep { get; set; }
	}
}
