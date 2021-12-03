using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PeriodicTasks.DatabaseDrivenTaskLoading.Models	
{
	public class PeriodicWeekday
	{
        [Key]
        public virtual int PeriodicWeekdayId { get; set; }

        public virtual int PeriodicScheduleId { get; set; }

	    public virtual bool Wednesday { get; set; }

	    public virtual bool Tuesday { get; set; }

	    public virtual bool Thursday { get; set; }

	    public virtual bool Sunday { get; set; }

	    public virtual bool Saturday { get; set; }

	    public virtual bool Monday { get; set; }

	    public virtual bool Friday { get; set; }

        [ForeignKey("PeriodicScheduleId")]
	    public virtual PeriodicSchedule PeriodicSchedule { get; set; }
	}
}
