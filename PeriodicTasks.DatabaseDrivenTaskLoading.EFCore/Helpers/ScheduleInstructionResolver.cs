using System;
using System.Linq;
using System.Text;
using System.Threading;
using ITVComponents.DataAccess;
using ITVComponents.DataAccess.Parallel;
using ITVComponents.EFRepo;
using ITVComponents.Threading;
using PeriodicTasks.DatabaseDrivenTaskLoading.Models;
using PeriodicTasks.DbContext;

namespace PeriodicTasks.DatabaseDrivenTaskLoading.EFCore.Helpers
{
    public static class ScheduleInstructionResolver<TContext> where TContext: Microsoft.EntityFrameworkCore.DbContext, ITaskSchedulerContext
    {
        /// <summary>
        /// the local connectionbuffer for the current thread
        /// </summary>
        private static AsyncLocal<IContextBuffer> connection = new AsyncLocal<IContextBuffer>();

        /// <summary>
        /// Gets the Scheduler-instruction for a specific SchedulerRuleId
        /// </summary>
        /// <param name="schedulerRuleId">the id of the Scheduler-Rule</param>
        /// <returns>the scheduler-instruction for the given scheduler-rule</returns>
        public static string GetSchedulerInstruction(int schedulerRuleId)
        {
            using (connection.Value.AcquireContext<TContext>(out var db))
            {
                //var schedulerRuleParam = db.GetParameter("ScheduleId", schedulerRuleId);
                var periodicSchedule =
                    db.PeriodicSchedules.FirstOrDefault(n => n.PeriodicScheduleId == schedulerRuleId);
                if (periodicSchedule != null)
                {
                    var times = db.PeriodicTimes.Where(n =>n.PeriodicScheduleId == schedulerRuleId).ToArray();
                    var weekdays = db.PeriodicWeekDays.FirstOrDefault(n => n.PeriodicScheduleId == schedulerRuleId);
                    var monthDays = db.PeriodicMonthdays.Where(n => n.PeriodicScheduleId == schedulerRuleId).ToArray();
                    var months = db.PeriodicMonths.FirstOrDefault(n => n.PeriodicScheduleId == schedulerRuleId);
                    string tString = string.Join(";", from t in times select t.Time.Replace(":", ""));
                    string wdString = BuildWeekDays(weekdays);
                    string mdString = string.Join("",from t in monthDays select t.DayNum.ToString().PadLeft(2,'0'));
                    string mString = BuildMonths(months);
                    string retVal =
                        $"{periodicSchedule.Period}{periodicSchedule.FirstDate:yyyyMMdd}{tString}{wdString}{mdString}{mString}{periodicSchedule.Occurrence:00}{(periodicSchedule.Mod != 0 ? ("." + periodicSchedule.Mod.ToString().PadLeft(2, '0')) : "")}{((periodicSchedule.OnServiceStart) ? "t" : "")}";
                    return retVal;
                }

                return null;
            }

        }

        /// <summary>
        /// Begins a new auto-resolve session before items are fetched
        /// </summary>
        /// <param name="localBuffer">the connection buffer that can be used to open a connection to the database</param>
        /// <returns>a resource-lock that can be used to clear the local connection value</returns>
        internal static IResourceLock BeginAutoResolve(IContextBuffer localBuffer)
        {
            connection.Value = localBuffer;
            return new ImportContextLock();
        }

        /// <summary>
        /// Releases the local connectionbuffer
        /// </summary>
        private static void EndAutoResolve()
        {
            connection.Value = null;
        }

        /// <summary>
        /// Builds a Month-string from a periodicMonth-Record
        /// </summary>
        /// <param name="months">the month-record</param>
        /// <returns>a string that represents the settings of the given months-record</returns>
        private static string BuildMonths(PeriodicMonth months)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                if (months != null)
                {
                    if (months.Jan)
                        sb.Append("jan");
                    if (months.Feb)
                        sb.Append("feb");
                    if (months.Mar)
                        sb.Append("mar");
                    if (months.Apr)
                        sb.Append("apr");
                    if (months.May)
                        sb.Append("may");
                    if (months.Jun)
                        sb.Append("jun");
                    if (months.Jul)
                        sb.Append("jul");
                    if (months.Aug)
                        sb.Append("aug");
                    if (months.Sep)
                        sb.Append("sep");
                    if (months.Oct)
                        sb.Append("oct");
                    if (months.Nov)
                        sb.Append("nov");
                    if (months.Dec)
                        sb.Append("dec");
                }
                return sb.ToString();
            }
            finally
            {
                sb.Clear();
            }
        }

        /// <summary>
        /// builds a weekdays-string from a PeriodicWeekDays record
        /// </summary>
        /// <param name="weekDays">the weekdays-record</param>
        /// <returns>a string that represents the settings of this weekdays-record</returns>
        private static string BuildWeekDays(PeriodicWeekday weekDays)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                if (weekDays != null)
                {
                    if (weekDays.Monday)
                        sb.Append("mon");
                    if (weekDays.Tuesday)
                        sb.Append("tue");
                    if (weekDays.Wednesday)
                        sb.Append("wed");
                    if (weekDays.Thursday)
                        sb.Append("thu");
                    if (weekDays.Friday)
                        sb.Append("fri");
                    if (weekDays.Saturday)
                        sb.Append("sat");
                    if (weekDays.Sunday)
                        sb.Append("sun");
                }

                return sb.ToString();
            }
            finally
            {
                sb.Clear();
            }
        }

        /// <summary>
        /// Resource-Lock that is used to reset local connection buffers
        /// </summary>
        private class ImportContextLock : IResourceLock
        {
            /// <summary>
            /// Initializes a new instance of the ImportContextLock class
            /// </summary>
            public ImportContextLock()
            {
            }

            #region Implementation of IDisposable

            /// <summary>
            /// Führt anwendungsspezifische Aufgaben durch, die mit der Freigabe, der Zurückgabe oder dem Zurücksetzen von nicht verwalteten Ressourcen zusammenhängen.
            /// </summary>
            /// <filterpriority>2</filterpriority>
            public void Dispose()
            {
                EndAutoResolve();
            }

            #endregion

            #region Implementation of IResourceLock

            public void Exclusive(Action action)
            {
                action();
            }

            public T Exclusive<T>(Func<T> action)
            {
                return action();
            }

            public IDisposable PauseExclusive()
            {
                return new ExclusivePauseHelper(() => InnerLock?.PauseExclusive());
            }

            /// <summary>
            /// Gets the inner lock of this Resource Lock instance
            /// </summary>
            public IResourceLock InnerLock { get { return null; } }

            #endregion
        }
    }
}
