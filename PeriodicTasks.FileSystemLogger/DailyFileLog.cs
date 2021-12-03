using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ITVComponents.DataAccess.Extensions;
using ITVComponents.Logging;

namespace PeriodicTasks.FileSystemLogger
{
    public class DailyFileLog:FileLog
    {
        /// <summary>
        /// Initializes a new instance of the DailyFileLog class
        /// </summary>
        /// <param name="environment">the environment that is running tasks</param>
        /// <param name="logDirectory">the directory into which to create logs</param>
        /// <param name="fileRetention">the number of days to keep log-files before deletion</param>
        public DailyFileLog(PeriodicEnvironment environment, string logDirectory, int fileRetention) : base(environment, logDirectory, fileRetention)
        {
        }

        /// <summary>
        /// Gets a LogWriter for a specific task
        /// </summary>
        /// <param name="target">the target task for which to get the logWriter</param>
        /// <param name="logDirectory">the directory into which the demanded file is created</param>
        /// <returns>a TextWriter instance that points to an appropriate file</returns>
        protected override TextWriter CreateWriter(PeriodicTask target, string logDirectory)
        {
            string logFileName = string.Format(@"{0}\{1}_{2:yyyyMMdd}.log", logDirectory, target.Name,
                                               DateTime.Today);
            var writer = new StreamWriter(logFileName, true, System.Text.Encoding.Default);
            writer.AutoFlush = true;
            return writer;
        }

        /// <summary>
        /// Runs a Housekeep job on the FileSystem
        /// </summary>
        /// <param name="logDirectory">the directory that holds all log-files</param>
        /// <param name="fileRetention">the number of days to keep log-files before deletion</param>
        protected override void HousekeepLogs(string logDirectory, int fileRetention)
        {
            var files = Directory.GetFiles(logDirectory, "*.log");
            (from f in files where DateTime.Now.Subtract(File.GetLastWriteTime(f)).TotalDays > fileRetention select f)
                .ForEach(TryDelete);
        }

        /// <summary>
        /// Tries to delete a file and logs a message on fails
        /// </summary>
        /// <param name="fileName">the fileName to delete</param>
        private void TryDelete(string fileName)
        {
            try
            {
                File.Delete(fileName);
            }
            catch (Exception ex)
            {
                LogEnvironment.LogEvent(string.Format("Failed to delete Logfile: {0}", ex),LogSeverity.Error);
            }
        }
    }
}
