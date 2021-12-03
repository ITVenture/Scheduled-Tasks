using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ITVComponents.DataAccess;
using ITVComponents.DataAccess.Parallel;
using ITVComponents.Threading;

namespace PeriodicTasks.DatabaseDrivenTaskLoading.Helpers
{
    public static class ScheduleInstructionResolver
    {
        /// <summary>
        /// the local connectionbuffer for the current thread
        /// </summary>
        private static AsyncLocal<IConnectionBuffer> connection = new AsyncLocal<IConnectionBuffer>();

        /// <summary>
        /// Gets the Scheduler-instruction for a specific SchedulerRuleId
        /// </summary>
        /// <param name="schedulerRuleId">the id of the Scheduler-Rule</param>
        /// <returns>the scheduler-instruction for the given scheduler-rule</returns>
        public static string GetSchedulerInstruction(int schedulerRuleId)
        {
            IDbWrapper db;
            using (connection.Value.AcquireConnection(false, out db))
            {
                var schedulerRuleParam = db.GetParameter("ScheduleId", schedulerRuleId);
                DynamicResult periodicSchedule =
                    db.GetNativeResults(
                        $"Select * from PeriodicSchedules where PeriodicScheduleId = {schedulerRuleParam.ParameterName}",null,
                        schedulerRuleParam).FirstOrDefault();
                if (periodicSchedule != null)
                {
                    var times = db.GetNativeResults(
                        $"Select * from PeriodicTimes where PeriodicScheduleId ={schedulerRuleParam.ParameterName}", null,
                        schedulerRuleParam);
                    var weekdays =
                        db.GetNativeResults(
                            $"Select * from PeriodicWeekdays where PeriodicScheduleId = {schedulerRuleParam.ParameterName}",
                            null,
                            schedulerRuleParam).FirstOrDefault();
                    var monthDays =
                        db.GetNativeResults(
                            $"Select * from PeriodicMonthDays where PeriodicScheduleId = {schedulerRuleParam.ParameterName}",
                            null,
                            schedulerRuleParam);
                    var months =
                        db.GetNativeResults(
                            $"Select * from PeriodicMonths where PeriodicScheduleId = {schedulerRuleParam.ParameterName}", null,
                            schedulerRuleParam).FirstOrDefault();
                    string tString = string.Join(";", from t in times select t["Time"].Replace(":", ""));
                    string wdString = BuildWeekDays(weekdays);
                    string mdString = string.Join("",from t in monthDays select t["dayNum"].ToString().PadLeft(2,'0'));
                    string mString = BuildMonths(months);
                    string retVal =
                        $"{periodicSchedule["Period"]}{periodicSchedule["firstDate"]:yyyyMMdd}{tString}{wdString}{mdString}{mString}{periodicSchedule["Occurrence"]:00}{(periodicSchedule["Mod"] != 0 ? ("." + periodicSchedule["Mod"].ToString().PadLeft(2, '0')) : "")}{((periodicSchedule["OnServiceStart"] ?? false) ? "t" : "")}";
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
        internal static IResourceLock BeginAutoResolve(IConnectionBuffer localBuffer)
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
        private static string BuildMonths(DynamicResult months)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                if (months != null)
                {
                    if (months["Jan"])
                        sb.Append("jan");
                    if (months["Feb"])
                        sb.Append("feb");
                    if (months["Mar"])
                        sb.Append("mar");
                    if (months["Apr"])
                        sb.Append("apr");
                    if (months["May"])
                        sb.Append("may");
                    if (months["Jun"])
                        sb.Append("jun");
                    if (months["Jul"])
                        sb.Append("jul");
                    if (months["Aug"])
                        sb.Append("aug");
                    if (months["Sep"])
                        sb.Append("sep");
                    if (months["Oct"])
                        sb.Append("oct");
                    if (months["Nov"])
                        sb.Append("nov");
                    if (months["Dec"])
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
        private static string BuildWeekDays(DynamicResult weekDays)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                if (weekDays != null)
                {
                    if (weekDays["Monday"])
                        sb.Append("mon");
                    if (weekDays["Tuesday"])
                        sb.Append("tue");
                    if (weekDays["Wednesday"])
                        sb.Append("wed");
                    if (weekDays["Thursday"])
                        sb.Append("thu");
                    if (weekDays["Friday"])
                        sb.Append("fri");
                    if (weekDays["Saturday"])
                        sb.Append("sat");
                    if (weekDays["Sunday"])
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

            /// <summary>
            /// Gets the inner lock of this Resource Lock instance
            /// </summary>
            public IResourceLock InnerLock { get { return null; } }

            #endregion
        }
    }
}
