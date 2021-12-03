using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PeriodicTasks.DatabaseDrivenTaskLoading.Models	
{
	public class PeriodicMonthday
	{
        [Key]
	    public virtual int PeriodicMonthdayId { get; set; }

	    public virtual int DayNum { get; set; }

	    public virtual int PeriodicScheduleId { get; set; }

        [ForeignKey("PeriodicScheduleId")]
	    public virtual PeriodicSchedule PeriodicSchedule { get; set; }
	}
}
