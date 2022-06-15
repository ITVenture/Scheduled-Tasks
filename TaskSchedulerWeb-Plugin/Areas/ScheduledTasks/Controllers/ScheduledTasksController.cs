using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using ITVComponents.DataAccess.Extensions;
using ITVComponents.Decisions.Entities.Results;
using ITVComponents.Helpers;
using ITVComponents.Plugins;
using ITVComponents.WebCoreToolkit.EntityFramework.DataAnnotations;
using ITVComponents.WebCoreToolkit.EntityFramework.Extensions;
using ITVComponents.WebCoreToolkit.EntityFramework.Helpers;
using ITVComponents.WebCoreToolkit.MvcExtensions;
using ITVComponents.WebCoreToolkit.WebPlugins.InjectablePlugins;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using PeriodicTasks;
using PeriodicTasks.DatabaseDrivenTaskLoading.Models;
using PeriodicTasks.DbContext;
using PeriodicTasks.FrontendHelpers;
using PeriodicTasks.FrontendHelpers.Models;
using PeriodicTasks.Remote;
using PeriodicTask = PeriodicTasks.DatabaseDrivenTaskLoading.Models.PeriodicTask;

namespace TaskSchedulerWeb.Areas.ScheduledTasks.Controllers
{
    [Authorize(Policy = "HasPermission(PeriodicTask.View,PeriodicTask.Write)"),Area("ScheduledTasks")]
    public class ScheduledTasksController : Controller
    {
        private TaskSchedulerContext context;
        
        private IInjectablePlugin<ServiceContextConnector> serviceContext;
        private bool? connected;

        public ScheduledTasksController(IInjectablePlugin<TaskSchedulerContext> db, IInjectablePlugin<ServiceContextConnector> serviceContext)
        {
            this.serviceContext = serviceContext;
            this.context = db.Instance;
        }

        private bool Connected => connected ??= ServiceContext != null;

        private ServiceContextConnector ServiceContext
        {
            get
            {
                try
                {
                    return serviceContext.Instance; //= serviceContext ?? GetScheduler(PluginFactory);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.OutlineException());
                }

                return null;
            }
        }

        public ActionResult Index()
        {
            PopulateForeignKeys();
            ViewData["Connected"] = Connected;
            return View();
        }

        private void PopulateForeignKeys()
        {
            var periods = new List<SelectListItem>();

            periods.Add(new SelectListItem { Text = "once", Value = "o" });
            periods.Add(new SelectListItem { Text = "Second-based", Value = "s" });
            periods.Add(new SelectListItem { Text = "Minute-based", Value = "i" });
            periods.Add(new SelectListItem { Text = "Hour-based", Value = "h" });
            periods.Add(new SelectListItem { Text = "Day-based", Value = "d" });
            periods.Add(new SelectListItem { Text = "Week-based", Value = "w" });
            periods.Add(new SelectListItem { Text = "Month-based", Value = "m" });
            periods.Add(new SelectListItem { Text = "Year-based", Value = "y" });

            ViewData["Periods"] = periods;

            var occurrences = new List<SelectListItem>();
            for (int i = 1; i <= 99; i++)
            {
                occurrences.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }

            ViewData["Occurrences"] = occurrences;


            var modulus = new List<SelectListItem>();
            for (int i = 0; i <= 99; i++)
            {
                modulus.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }

            ViewData["Moduluses"] = modulus;

            ViewData["ParameterTypes"] = new[]
                                             {
                                                 new SelectListItem {Text = "Literal", Value = "Literal"},
                                                 new SelectListItem {Text = "Expression", Value = "Expression"},
                                                 new SelectListItem {Text = "Variable", Value = "Variable"}
                                             };
            //{
                ViewData["Workers"] = new SelectListItem[] {};
                if (ServiceContext != null)
                {
                    var workers = ServiceContext.GetActiveWorkers();
                    ViewData["Workers"] = (from t in workers select new SelectListItem {Text = t, Value = t}).ToArray();
                }
            //}
        }

        #region scheduler-Grid Methods
        
        public ActionResult GetScheduledTasks([DataSourceRequest]DataSourceRequest request)
        {
            TaskModel[] serviceTasks = { };

            if (Connected)
            {
                try
                {
                    serviceTasks =
                        ServiceContext.GetScheduledTasks();
                    Console.WriteLine($"{serviceTasks?.Length} Tasks received");
                    if (serviceTasks != null)
                    {
                        foreach (var t in serviceTasks)
                        {
                            Console.WriteLine($"{t.TaskId}\t{t.TaskName}\t{t.Remarks}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.OutlineException());
                }
            }

            var knownQueries = new Dictionary<string, DiagnoseQueryHelper.DiagQueryItem>();
            DiagnoseQueryHelper.DiagEntityAnlyseItem[] tmpDiag = null;
            return Json(context.PeriodicTasks.ToDataSourceResult(request,
                f => HttpContext.RequestServices.DiagnoseResult<PeriodicTask, PeriodicTaskViewModel>(f, new Dictionary<string,object>
                {
                    {"PeriodicTaskId",f.PeriodicTaskId}
                },knownQueries, ref tmpDiag ,(t,v) => EnrichSchedulerInformation(serviceTasks, t, v))));
        }

        
        public async Task<ActionResult> UpdateScheduledTask([DataSourceRequest] DataSourceRequest request, int PeriodicTaskId)
        {
            var dbtoUpdate = context.PeriodicTasks.First(db => db.PeriodicTaskId == PeriodicTaskId);
            var scheduler = dbtoUpdate.PeriodicSchedules.FirstOrDefault()?.SchedulerName;
            TaskModel[] serviceTasks =null;
            if (Connected && !string.IsNullOrEmpty(scheduler))
            {
                serviceTasks = ServiceContext?.GetScheduledTasks(scheduler);
            }
            if (ModelState.IsValid)
            {
                //context.Entry(context.PeriodicTasks.First(x => x.PeriodicTaskId == PeriodicTaskId)).CurrentValues.SetValues(dbtoUpdate);
                await this.TryUpdateModelAsync<PeriodicTaskViewModel,PeriodicTask>(dbtoUpdate);
                await context.SaveChangesAsync();
            }

            return Json(new[]
            {
                dbtoUpdate.ToViewModel<PeriodicTask, PeriodicTaskViewModel>((t, v) =>
                    EnrichSchedulerInformation(serviceTasks, t, v))
            }.ToDataSourceResult(request));
        }


        
        public ActionResult InsertScheduledTask([DataSourceRequest] DataSourceRequest request, PeriodicTask periodictask)
        {


            context.PeriodicTasks.Add(periodictask);
            context.SaveChanges();


            return Json(new[]{periodictask.ToViewModel<PeriodicTask,PeriodicTaskViewModel>()}.ToDataSourceResult(request));
        }

        
        public ActionResult DeleteScheduledTask([DataSourceRequest] DataSourceRequest request,  int PeriodicTaskId)
        {
            var dataToDelete = context.PeriodicTasks.First(a => a.PeriodicTaskId == PeriodicTaskId);
            try
            {
                context.PeriodicTasks.Remove(dataToDelete);
                context.SaveChanges();
            }
            catch (Exception e)
            {
                string Message = e.Message;
                Exception currentEx = e;
                while (currentEx.InnerException != null)
                {
                    currentEx = currentEx.InnerException;
                    Message = currentEx.Message;
                }

                ModelState.AddModelError("Error", Message);
                //return Json(GetGridData().ToDataSourceResult(request, ModelState));
            }


            return Json(new[]{dataToDelete.ToViewModel<PeriodicTask, PeriodicTaskViewModel>()}.ToDataSourceResult(request, ModelState));
        }

        
        public string RunTask(int periodicTaskId)
        {
            string message = "OK";
            try
            {
                var task = context.PeriodicTasks.First(n => n.PeriodicTaskId == periodicTaskId);
                var schedulerName = task.PeriodicSchedules.FirstOrDefault()?.SchedulerName;
                if (Connected && !string.IsNullOrEmpty(schedulerName))
                {
                    ServiceContext.RunTask(schedulerName, periodicTaskId);
                }
                else
                {
                    message = "Nicht verbunden oder Task nicht geplant";
                }
                //}
            }
            catch (Exception e)
            {
                message = e.Message;
                Exception currentEx = e;
                while (currentEx.InnerException != null)
                {
                    currentEx = currentEx.InnerException;
                    message = currentEx.Message;
                }
            }

            return message;
        }

        #endregion

        #region time-grid Methods
        
        public ActionResult GetTimes([DataSourceRequest]DataSourceRequest request, int pPeriodicScheduleId)
        {
            return Json(context.PeriodicTimes.Where(n => n.PeriodicScheduleId == pPeriodicScheduleId).ToDataSourceResult(request, n => n.ToViewModel<PeriodicTime,PeriodicScheduleTimeViewModel>()));
        }

        
        public async Task<ActionResult> UpdateTime([DataSourceRequest] DataSourceRequest request, int PeriodicTimeId, int pPeriodicScheduleId)
        {

            var dbtoUpdate = context.PeriodicTimes.First(db => db.PeriodicTimeId == PeriodicTimeId);

            if (ModelState.IsValid)
            {
                await this.TryUpdateModelAsync<PeriodicScheduleTimeViewModel, PeriodicTime>(dbtoUpdate);
                //context.Entry(context.PeriodicTimes.First(x => x.PeriodicTimeId == PeriodicTimeId)).CurrentValues.SetValues(dbtoUpdate);
                await context.SaveChangesAsync();
            }

            return Json(new[]{dbtoUpdate.ToViewModel<PeriodicTime,PeriodicScheduleTimeViewModel>()}.ToDataSourceResult(request));
        }


        
        public ActionResult InsertTime([DataSourceRequest] DataSourceRequest request, PeriodicTime periodictime, int pPeriodicScheduleId)
        {

            periodictime.PeriodicScheduleId = pPeriodicScheduleId;
            context.PeriodicTimes.Add(periodictime);
            context.SaveChanges();


            return Json(new[]{periodictime.ToViewModel<PeriodicTime,PeriodicScheduleTimeViewModel>()}.ToDataSourceResult(request));
        }

        
        public ActionResult DeleteTime([DataSourceRequest] DataSourceRequest request, int PeriodicTimeId, int pPeriodicScheduleId)
        {
            var dataToDelete = context.PeriodicTimes.First(a => a.PeriodicTimeId == PeriodicTimeId);
            try
            {    
                context.PeriodicTimes.Remove(dataToDelete);
                context.SaveChanges();
            }
            catch (Exception e)
            {
                string Message = e.Message;
                Exception currentEx = e;
                while (currentEx.InnerException != null)
                {
                    currentEx = currentEx.InnerException;
                    Message = currentEx.Message;
                }

                ModelState.AddModelError("Error", Message);
                //return Json(GetGridData().ToDataSourceResult(request, ModelState));
            }

            return Json(new[]{dataToDelete.ToViewModel<PeriodicTime,PeriodicScheduleTimeViewModel>()}.ToDataSourceResult(request));
        }
        #endregion

        #region montday-grid Methods
        
        public ActionResult GetMonthDays([DataSourceRequest]DataSourceRequest request, int pPeriodicScheduleId)
        {
            return Json(context.PeriodicMonthdays.Where(n => n.PeriodicScheduleId == pPeriodicScheduleId).ToDataSourceResult(request, n => n.ToViewModel<PeriodicMonthday,PeriodicMonthdayViewModel>()));
        }

        
        public async Task<ActionResult> UpdateMonthDay([DataSourceRequest] DataSourceRequest request, int PeriodicMonthdayId, int pPeriodicScheduleId)
        {

            var dbtoUpdate = context.PeriodicMonthdays.First(db => db.PeriodicMonthdayId == PeriodicMonthdayId);
            if (ModelState.IsValid)
            {
                await this.TryUpdateModelAsync<PeriodicMonthdayViewModel,PeriodicMonthday>(dbtoUpdate);
                //context.Entry(context.PeriodicMonthdays.First(x => x.PeriodicMonthdayId == PeriodicMonthdayId)).CurrentValues.SetValues(dbtoUpdate);
                await context.SaveChangesAsync();
            }

            return Json(new[]{dbtoUpdate.ToViewModel<PeriodicMonthday,PeriodicMonthdayViewModel>()}.ToDataSourceResult(request));
        }


        
        public ActionResult InsertMonthDay([DataSourceRequest] DataSourceRequest request, PeriodicMonthday periodicmonthday, int pPeriodicScheduleId)
        {

            periodicmonthday.PeriodicScheduleId = pPeriodicScheduleId;
            context.PeriodicMonthdays.Add(periodicmonthday);
            context.SaveChanges();


            return Json(new[]{periodicmonthday.ToViewModel<PeriodicMonthday,PeriodicMonthdayViewModel>()}.ToDataSourceResult(request));
        }

        
        public ActionResult DeleteMonthDay([DataSourceRequest] DataSourceRequest request, int PeriodicMonthdayId, int pPeriodicScheduleId)
        {
            var dataToDelete = context.PeriodicMonthdays.First(a => a.PeriodicMonthdayId == PeriodicMonthdayId);
            try
            {
                context.PeriodicMonthdays.Remove(dataToDelete);
                context.SaveChanges();
            }
            catch (Exception e)
            {
                string Message = e.Message;
                Exception currentEx = e;
                while (currentEx.InnerException != null)
                {
                    currentEx = currentEx.InnerException;
                    Message = currentEx.Message;
                }

                ModelState.AddModelError("Error", Message);
                //return Json(GetGridData().ToDataSourceResult(request, ModelState));
            }


            return Json(new[]{dataToDelete.ToViewModel<PeriodicMonthday,PeriodicMonthdayViewModel>()}.ToDataSourceResult(request));
        }

        #endregion

        #region month-grid Methods
        public JsonResult UpdateMonth(string Month, bool isActive, int PeriodicScheduleId)
        {
            var weekdays = context.PeriodicMonths.FirstOrDefault(w => w.PeriodicScheduleId == PeriodicScheduleId);
            if (weekdays == null)
            {
                PeriodicMonth pw = new PeriodicMonth();
                AssignMonth(Month, pw, isActive);
                pw.PeriodicScheduleId = PeriodicScheduleId;
                context.PeriodicMonths.Add(pw);
            }
            else
            {
                AssignMonth(Month, weekdays, isActive);
            }
            context.SaveChanges();
            return Json(new { message = "ok" });
        }
        #endregion

        #region weekday-grid Methods
        public JsonResult UpdateWeekday(string Day, bool isActive, int PeriodicScheduleId)
        {
            var weekdays = context.PeriodicWeekDays.FirstOrDefault(w => w.PeriodicScheduleId == PeriodicScheduleId);
            if (weekdays == null)
            {
                PeriodicWeekday pw = new PeriodicWeekday();
                AssignDay(Day, pw, isActive);
                pw.PeriodicScheduleId = PeriodicScheduleId;
                context.PeriodicWeekDays.Add(pw);
            }
            else
            {
                AssignDay(Day, weekdays, isActive);
            }
            context.SaveChanges();
            return Json(new { message = "ok" });
        }
        #endregion

        #region step-parameters-grid Methods
        
        public ActionResult GetStepParameters([DataSourceRequest] DataSourceRequest request, int pPeriodicStepId)
        {
            return Json(context.PeriodicStepParameters.Where(n => n.PeriodicStepId == pPeriodicStepId)
                .ToDataSourceResult(request, n => n.ToViewModel<PeriodicStepParameter, PeriodicStepParameterModel>()));
        }

        
        public async Task<ActionResult> UpdateStepParameter([DataSourceRequest] DataSourceRequest request,
                                       int PeriodicStepParameterId)
        {

            var dbtoUpdate =
                context.PeriodicStepParameters.First(
                    db => db.PeriodicStepParameterId == PeriodicStepParameterId);

            if (ModelState.IsValid)
            {
                await this.TryUpdateModelAsync<PeriodicStepParameterModel, PeriodicStepParameter>(dbtoUpdate);
                //context.Entry(context.PeriodicStepParameters.First(x => x.PeriodicStepParameterId == PeriodicStepParameterId)).CurrentValues.SetValues(dbtoUpdate);
                await context.SaveChangesAsync();
            }

            return Json(new[]{dbtoUpdate.ToViewModel<PeriodicStepParameter,PeriodicStepParameterModel>()}.ToDataSourceResult(request));
        }


        
        public ActionResult InsertStepParameter([DataSourceRequest] DataSourceRequest request,
                                       PeriodicStepParameter periodicStepParameter, int pPeriodicStepId)
        {

            periodicStepParameter.PeriodicStepId = pPeriodicStepId;
            context.PeriodicStepParameters.Add(periodicStepParameter);
            context.SaveChanges();


            return Json(new[]{periodicStepParameter.ToViewModel<PeriodicStepParameter,PeriodicStepParameterModel>()}.ToDataSourceResult(request));
        }

        
        public ActionResult DeleteStepParameter([DataSourceRequest] DataSourceRequest request,
                                       int PeriodicStepParameterId)
        {
            var dataToDelete =
                context.PeriodicStepParameters.First(
                    a => a.PeriodicStepParameterId == PeriodicStepParameterId);
            try
            {
                context.PeriodicStepParameters.Remove(dataToDelete);
                context.SaveChanges();
            }
            catch (Exception e)
            {
                string Message = e.Message;
                Exception currentEx = e;
                while (currentEx.InnerException != null)
                {
                    currentEx = currentEx.InnerException;
                    Message = currentEx.Message;
                }

                ModelState.AddModelError("Error", Message);
                //return Json(GetGridData().ToDataSourceResult(request, ModelState));
            }


            return Json(new[]{dataToDelete.ToViewModel<PeriodicStepParameter,PeriodicStepParameterModel>()}.ToDataSourceResult(request));
        }
        #endregion

        #region schedules-grid Methods
        
        public ActionResult GetSchedules([DataSourceRequest]DataSourceRequest request, int pPeriodicTaskId)
        {
            return Json(context.PeriodicSchedules.Where(n => n.PeriodicTaskId == pPeriodicTaskId).ToDataSourceResult(request, n=> n.ToViewModel<PeriodicSchedule,PeriodicScheduleTabViewModel>()));
        }

        
        public async Task<ActionResult> UpdateSchedule([DataSourceRequest] DataSourceRequest request, int PeriodicScheduleId, int pPeriodicTaskId)
        {

            var dbtoUpdate = context.PeriodicSchedules.First(db => db.PeriodicScheduleId == PeriodicScheduleId);
            if (ModelState.IsValid)
            {
                await this.TryUpdateModelAsync<PeriodicScheduleTabViewModel, PeriodicSchedule>(dbtoUpdate);
                //context.Entry(context.PeriodicSchedules.First(x => x.PeriodicScheduleId == PeriodicScheduleId)).CurrentValues.SetValues(dbtoUpdate);
                await context.SaveChangesAsync();
            }

            return Json(new[]{dbtoUpdate.ToViewModel<PeriodicSchedule,PeriodicScheduleTabViewModel>()}.ToDataSourceResult(request));
        }


        
        public ActionResult InsertSchedule([DataSourceRequest] DataSourceRequest request, PeriodicSchedule periodicschedule, int pPeriodicTaskId)
        {

            periodicschedule.PeriodicTaskId = pPeriodicTaskId;
            context.PeriodicSchedules.Add(periodicschedule);
            context.SaveChanges();

            return Json(new[]{periodicschedule.ToViewModel<PeriodicSchedule,PeriodicScheduleTabViewModel>()}.ToDataSourceResult(request));
        }

        
        public ActionResult DeleteSchedule([DataSourceRequest] DataSourceRequest request, int PeriodicScheduleId, int pPeriodicTaskId)
        {
            var dataToDelete = context.PeriodicSchedules.First(a => a.PeriodicScheduleId == PeriodicScheduleId);
            try
            {
                var weekDays =
                    context.PeriodicWeekDays.Where(n => n.PeriodicScheduleId == PeriodicScheduleId);
                var monthdays =
                    context.PeriodicMonthdays.Where(n => n.PeriodicScheduleId == PeriodicScheduleId);
                var months = context.PeriodicMonths.Where(n => n.PeriodicScheduleId == PeriodicScheduleId);
                var times = context.PeriodicTimes.Where(n => n.PeriodicScheduleId == PeriodicScheduleId);
                context.PeriodicWeekDays.RemoveRange(weekDays);
                context.PeriodicMonthdays.RemoveRange(monthdays);
                context.PeriodicMonths.RemoveRange(months);
                context.PeriodicTimes.RemoveRange(times);
                context.PeriodicSchedules.Remove(dataToDelete);
                context.SaveChanges();
            }
            catch (Exception e)
            {
                string Message = e.Message;
                Exception currentEx = e;
                while (currentEx.InnerException != null)
                {
                    currentEx = currentEx.InnerException;
                    Message = currentEx.Message;
                }

                ModelState.AddModelError("Error", Message);
                //return Json(GetGridData().ToDataSourceResult(request, ModelState));
            }


            return Json(new[]{dataToDelete.ToViewModel<PeriodicSchedule,PeriodicScheduleTabViewModel>()}.ToDataSourceResult(request));
        }
        #endregion

        #region Run-grid Methods
        
        public ActionResult GetRuns([DataSourceRequest]DataSourceRequest request, [FromQuery]int pPeriodicTaskId, [FromForm]bool rep, [FromForm]bool wrn, [FromForm]bool err, [FromForm]string tx)
        {
            var tmp = context.PeriodicRuns.Where(n => n.PeriodicTaskId == pPeriodicTaskId);
            List<int> li = new List<int>();
            if (rep)
            {
                li.Add((int)PeriodicTasks.LogMessageType.Report);
            }

            if (wrn)
            {
                li.Add((int)PeriodicTasks.LogMessageType.Warning);
            }

            if (err)
            {
                li.Add((int)PeriodicTasks.LogMessageType.Error);
            }

            if (li.Count != 0)
            {
                var arr = li.ToArray();
                tmp = tmp.Where(n => n.PeriodicLogs.Any(l => arr.Contains(l.MessageType)));
            }

            if (!string.IsNullOrEmpty(tx))
            {
                tmp = tmp.Where(t => t.PeriodicLogs.Any(l => l.Message.Contains(tx)));
            }

            return Json(tmp.ToDataSourceResult(request, n=> n.ToViewModel<PeriodicRun,PeriodicRunViewModel>()));
        }
        #endregion

        #region task-steps-grid Methods
        
        public ActionResult GetTaskSteps([DataSourceRequest]DataSourceRequest request, int pPeriodicTaskId)
        {
            return Json(context.PeriodicSteps.Where(n => n.PeriodicTaskId == pPeriodicTaskId).ToDataSourceResult(request, n => n.ToViewModel<PeriodicStep,PeriodicStepModel>()));
        }


        public ActionResult ReadTaskSteps(int pPeriodicTaskId)
        {
            return Json(context.PeriodicSteps.Where(n => n.PeriodicTaskId == pPeriodicTaskId).ToArray()
                .Select(n => n.ToViewModel<PeriodicStep, PeriodicStepMiniModel>()));
        }

        
        public async Task<ActionResult> UpdateTaskStep([DataSourceRequest] DataSourceRequest request, int PeriodicStepId, int pPeriodicTaskId)
        {

            var dbtoUpdate = context.PeriodicSteps.First(db => db.PeriodicStepId == PeriodicStepId);

            if (ModelState.IsValid)
            {
                await this.TryUpdateModelAsync<PeriodicStepModel,PeriodicStep>(dbtoUpdate);
                //context.Entry(context.PeriodicSteps.First(x => x.PeriodicStepId == PeriodicStepId)).CurrentValues.SetValues(dbtoUpdate);
                await context.SaveChangesAsync();
            }

            return Json(new[]{dbtoUpdate.ToViewModel<PeriodicStep,PeriodicStepModel>()}.ToDataSourceResult(request));
        }


        
        public ActionResult InsertTaskStep([DataSourceRequest] DataSourceRequest request, PeriodicStep periodicStep, int pPeriodicTaskId)
        {
            periodicStep.PeriodicTaskId = pPeriodicTaskId;
            periodicStep.StepOrder = context.PeriodicSteps.Count(n => n.PeriodicTaskId == periodicStep.PeriodicTaskId);
            context.PeriodicSteps.Add(periodicStep);
            context.SaveChanges();


            return Json(new[]{periodicStep.ToViewModel<PeriodicStep,PeriodicStepModel>()}.ToDataSourceResult(request));
        }

        
        public ActionResult DeleteTaskStep([DataSourceRequest] DataSourceRequest request, int PeriodicStepId, int pPeriodicTaskId)
        {
            var dataToDelete = context.PeriodicSteps.First(a => a.PeriodicStepId == PeriodicStepId);
            try
            {
                context.PeriodicSteps.Remove(dataToDelete);
                context.SaveChanges();
            }
            catch (Exception e)
            {
                string Message = e.Message;
                Exception currentEx = e;
                while (currentEx.InnerException != null)
                {
                    currentEx = currentEx.InnerException;
                    Message = currentEx.Message;
                }

                ModelState.AddModelError("Error", Message);
                //return Json(GetGridData().ToDataSourceResult(request, ModelState));
            }


            return Json(new[]{dataToDelete.ToViewModel<PeriodicStep,PeriodicStepModel>()}.ToDataSourceResult(request));
        }
        #endregion

        #region log-grid Methods
        
        public ActionResult GetLogEntries([DataSourceRequest]DataSourceRequest request, int pPeriodicRunId)
        {
            return Json(context.PeriodicLog.Where(n => n.PeriodicRunId == pPeriodicRunId).ToDataSourceResult(request, n => n.ToViewModel<PeriodicLog,PeriodicLogViewModel>((m,v) => v.MessageType = (LogMessageType)m.MessageType)));
        }
        #endregion

        public PartialViewResult Details(int PeriodicScheduleId)
        {
            var monthdays = new List<SelectListItem>();
            for (int i = 1; i <= 31; i++)
            {
                monthdays.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            monthdays.Add(new SelectListItem { Text = "-1", Value = "-1" });
            ViewData["Monthdays"] = monthdays;
            ViewData["Months"] = context.PeriodicMonths.FirstOrDefault(w => w.PeriodicScheduleId == PeriodicScheduleId);
            ViewData["WeekDays"] = context.PeriodicWeekDays.FirstOrDefault(d => d.PeriodicScheduleId == PeriodicScheduleId);

            var schedule = context.PeriodicSchedules.FirstOrDefault(s => s.PeriodicScheduleId == PeriodicScheduleId);
            return PartialView("ScheduleDetails", schedule);
        }

        public PartialViewResult StepDetails(int PeriodicStepId)
        {
            PopulateForeignKeys();
            PeriodicStep step = context.PeriodicSteps.FirstOrDefault(n => n.PeriodicStepId == PeriodicStepId);
            WorkerDescription description = null;
            if (step != null)
            {
                    if (ServiceContext != null)
                    {
                        description = ServiceContext.DescribeWorker(step.WorkerName);
                    }
                //}
            }

            return PartialView(new WorkerDescriptionModel(PeriodicStepId, description));
        }

        public PartialViewResult _History(int PeriodicTaskId)
        {
            ViewData["PeriodicTaskId"] = PeriodicTaskId;
            return PartialView();
        }

        public PartialViewResult _ScheduleRoot(int periodicTaskId)
        {
            PopulateForeignKeys();
            ViewData["PeriodicTaskId"] = periodicTaskId;
            return PartialView();
        }

        public PartialViewResult _StepRoot(int periodicTaskId)
        {
            PopulateForeignKeys();
            ViewData["PeriodicTaskId"] = periodicTaskId;
            return PartialView();
        }

        public PartialViewResult _LogTable(int periodicRunId)
        {
            ViewData["PeriodicRunId"] = periodicRunId;
            return PartialView();
        }

        private void AssignMonth(string Month, PeriodicMonth pw, bool isActive)
        {
            switch (Month.ToLower())
            {
                case "jan":
                    pw.Jan = isActive;
                    break;
                case "feb":
                    pw.Feb = isActive;
                    break;
                case "mar":
                    pw.Mar = isActive;
                    break;
                case "apr":
                    pw.Apr = isActive;
                    break;
                case "may":
                    pw.May = isActive;
                    break;
                case "jun":
                    pw.Jun = isActive;
                    break;
                case "jul":
                    pw.Jul = isActive;
                    break;
                case "aug":
                    pw.Aug = isActive;
                    break;
                case "sep":
                    pw.Sep = isActive;
                    break;
                case "oct":
                    pw.Oct = isActive;
                    break;
                case "nov":
                    pw.Nov = isActive;
                    break;
                case "dec":
                    pw.Dec = isActive;
                    break;
            }
        }

        private void AssignDay(string Day, PeriodicWeekday pw, bool isActive)
        {
            switch (Day.ToLower())
            {
                case "monday":
                    pw.Monday = isActive;
                    break;
                case "tuesday":
                    pw.Tuesday = isActive;
                    break;
                case "wednesday":
                    pw.Wednesday = isActive;
                    break;
                case "thursday":
                    pw.Thursday = isActive;
                    break;
                case "friday":
                    pw.Friday = isActive;
                    break;
                case "saturday":
                    pw.Saturday = isActive;
                    break;
                case "sunday":
                    pw.Sunday = isActive;
                    break;
            }
        }

        private void EnrichSchedulerInformation(TaskModel[] serviceTasks, PeriodicTask periodicTask, PeriodicTaskViewModel periodicTaskViewModel)
        {
            if (serviceTasks != null)
            {
                TaskModel src = serviceTasks.FirstOrDefault(n => n.TaskId == periodicTask.PeriodicTaskId);
                if (src != null)
                {
                    periodicTaskViewModel.Remarks = src.Remarks;
                    periodicTaskViewModel.Pushable = true;
                }
            }
        }

        #region classes
        public class WorkerDescriptionModel
        {
            public int PeriodicStepId { get; set; }
            public string Description { get; set; }
            public string Remarks { get; set; }
            public ParameterDescription[] ExpectedParameters { get; set; }
            public string CommandDescription { get; set; }
            public string ReturnType { get; set; }
            public string Type { get; set; }
            public string ReturnDescription { get; set; }
            private WorkerDescription description;

            public WorkerDescriptionModel(int periodicStepId, WorkerDescription description)
            {
                PeriodicStepId = periodicStepId;
                Description = description?.Description;
                Remarks = description?.Remarks;
                CommandDescription = description?.CommandDescription;
                ExpectedParameters = description?.ExpectedParameters;
                ReturnType = description?.ReturnType;
                ReturnDescription = description?.ReturnDescription;
                Type = description?.Type;
            }
        }

        public class PeriodicTaskViewModel
        {
            public int PeriodicTaskId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public bool Active { get; set; }
            public int Priority { get; set; }
            public string Remarks { get; set; }
            public string ExclusiveAreaName { get; set; }
            public bool Pushable { get; set; }

            [DiagnosticResult("ScheduledTaskDiag")]
            public SimpleTriStateResult TaskDiagnostics { get; set; }
        }

        public class PeriodicScheduleTimeViewModel
        {
            public int PeriodicTimeId { get; set; }
            public int PeriodicScheduleId { get; set; }
            [UIHint("PeriodicTimes")]
            [RegularExpression(@"^([0-9]|0[0-9\*]|1[0-9\*]|2[0-3\*]):[0-5\*][0-9\*]$", ErrorMessage = "Use this Format: HH:mm")]
            public string Time { get; set; }
        }

        public class PeriodicMonthdayViewModel
        {
            public int PeriodicMonthdayId { get; set; }
            public int PeriodicScheduleId { get; set; }
            public int DayNum { get; set; }
        }

        public class PeriodicStepParameterModel
        {

            public int PeriodicStepParameterId { get; set; }
            public string ParameterName { get; set; }
            public string ParameterType { get; set; }
            public string Settings { get; set; }
            public string Value { get; set; }
            public int PeriodicStepId { get; set; }
        }

        public class PeriodicScheduleTabViewModel
        {
            public int PeriodicScheduleId { get; set; }
            [Required]
            public string Name { get; set; }
            [Required]
            public string Period { get; set; }
            [DataType(DataType.Date)]
            public DateTime FirstDate { get; set; }
            [Required]
            public int Occurrence { get; set; }
            public int Mod { get; set; }
            public int PeriodicTaskId { get; set; }
            public string SchedulerName { get; set; }
        }

        public class PeriodicRunViewModel
        {

            public int PeriodicRunId { get; set; }
            [UIHint("DateTime")]
            public DateTime StartTime { get; set; }
            [UIHint("DateTime")]
            public DateTime? EndTime { get; set; }
            public int PeriodicTaskId { get; set; }
        }

        public class PeriodicStepModel
        {

            public int PeriodicStepId { get; set; }
            public string Name { get; set; }
            public string WorkerName { get; set; }

            [DataType(DataType.MultilineText)]
            public string Command { get; set; }

            public string OutputVariable { get; set; }
            public int StepOrder { get; set; }
            public int PeriodicTaskId { get; set; }
            public string ExclusiveAreaName { get; set; }
        }

        public class PeriodicStepMiniModel
        {
            public int PeriodicStepId { get; set; }
            public string Name { get; set; }
        }

        public class PeriodicLogViewModel
        {

            public int PeriodicLogId { get; set; }
            public int? PeriodicStepId { get; set; }
            public int PeriodicRunId { get; set; }
            public string Message { get; set; }
            public LogMessageType MessageType { get; set; }
            public DateTime LogTime { get; set; }
        }
        #endregion
    }
}
