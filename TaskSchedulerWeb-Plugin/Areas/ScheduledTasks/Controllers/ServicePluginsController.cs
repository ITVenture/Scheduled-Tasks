using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ITVComponents.DataAccess.Extensions;
using ITVComponents.Plugins;
using ITVComponents.Plugins.DatabaseDrivenConfiguration.Models;
using ITVComponents.Plugins.PluginServices;
using ITVComponents.WebCoreToolkit.Security;
using ITVComponents.WebCoreToolkit.WebPlugins.InjectablePlugins;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PeriodicTasks.DbContext;
using PeriodicTasks.FrontendHelpers;
using TaskSchedulerWeb.Options;

namespace TaskSchedulerWeb.Areas.ScheduledTasks.Controllers
{
    [Authorize(Policy = "HasPermission(PeriodicTask.View,PeriodicTask.Write)"), Area("ScheduledTasks")]
    public class ServicePluginsController : Controller
    {
        private readonly IOptions<TaskUiOptions> uiOptions;
        private readonly IPermissionScope currentScopeProvider;
        private TaskSchedulerContext context;

        public ServicePluginsController(IInjectablePlugin<TaskSchedulerContext> db, IOptions<TaskUiOptions> uiOptions, IPermissionScope currentScopeProvider)
        {
            this.uiOptions = uiOptions;
            this.currentScopeProvider = currentScopeProvider;
            this.context = db.Instance;
        }

        public ActionResult Index()
        {
            return View();
        }

        public PartialViewResult Details(string assemblyName)
        {

            return PartialView("Details", assemblyName);

        }


        public ActionResult GetData([DataSourceRequest]DataSourceRequest request)
        {
            return Json(context.Plugins.ToDataSourceResult(request, n => n.ToViewModel<DatabasePlugin,ServicePluginViewModel>()));
        }

        
        public ActionResult UpdateData([DataSourceRequest] DataSourceRequest request, ServicePluginViewModel plugin)
        {

            context.Entry(context.Plugins.First(x => x.PluginId == plugin.PluginId)).CurrentValues.SetValues(plugin);
            context.SaveChanges();

            return Json(new[]{ plugin }.ToDataSourceResult(request, ModelState));
        }


        
        public ActionResult InsertData([DataSourceRequest] DataSourceRequest request, DatabasePlugin plugin)
        {

            plugin.LoadOrder = context.Plugins.Count();
            if (uiOptions.Value.UseTenantSupport)
            {
                plugin.TenantId = currentScopeProvider.PermissionPrefix;
            }

            context.Plugins.Add(plugin);
            context.SaveChanges();


            return Json(new[]{plugin.ToViewModel<DatabasePlugin,ServicePluginViewModel>()}.ToDataSourceResult(request, ModelState));
        }

        
        public ActionResult DeleteData([DataSourceRequest] DataSourceRequest request, int PluginId)
        {
            var dataToDelete = context.Plugins.First(a => a.PluginId == PluginId);
            try
            {
                context.Plugins.Remove(dataToDelete);
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


            return Json(new[]{dataToDelete.ToViewModel<DatabasePlugin,ServicePluginViewModel>()}.ToDataSourceResult(request, ModelState));
        }

        public class ServicePluginViewModel
        {
            public int PluginId { get; set; }
            public int LoadOrder { get; set; }
            public string UniqueName { get; set; }
            
            public string Constructor { get; set; }
            public bool Disabled { get; set; }
            [DataType(DataType.MultilineText)]
            public string DisabledReason { get; set; }
        }   
    }
}