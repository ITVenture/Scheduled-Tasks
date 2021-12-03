using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PeriodicTasks.DatabaseDrivenTaskLoading.Models	
{
	public class PeriodicTime
	{
        [Key]
	    public virtual int PeriodicTimeId { get; set; }

	    public virtual string Time { get; set; }

	    public virtual int PeriodicScheduleId { get; set; }

        [ForeignKey("PeriodicScheduleId")]
	    public virtual PeriodicSchedule PeriodicSchedule { get; set; }
	}
}
