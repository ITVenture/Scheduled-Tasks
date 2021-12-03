using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PeriodicTasks.DatabaseDrivenTaskLoading.Models	
{
	public class PeriodicStepParameter
	{
        [Key]
	    public virtual int PeriodicStepParameterId { get; set; }

	    public virtual int PeriodicStepId { get; set; }

	    public virtual string ParameterName { get; set; }

	    public virtual string ParameterType { get; set; }

	    public virtual string Value { get; set; }

	    public virtual string Settings { get; set; }

        [ForeignKey("PeriodicStepId")]
	    public virtual PeriodicStep PeriodicStep { get; set; }
	}
}
