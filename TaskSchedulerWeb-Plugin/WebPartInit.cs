using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITVComponents.Settings.Native;
using ITVComponents.WebCoreToolkit.AspExtensions;
using ITVComponents.WebCoreToolkit.AspExtensions.Impl;
using ITVComponents.WebCoreToolkit.Net.TelerikUi.Options;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskSchedulerWeb.Extensions;
using TaskSchedulerWeb.Options;

namespace TaskSchedulerWeb
{
    [WebPart]
    public static class WebPartInit
    {

        [LoadWebPartConfig]
        public static TaskUiOptions LoadOptions(IConfiguration config, string path)
        {
            return config.GetSection<TaskUiOptions>(path);
        }

        [ServiceRegistrationMethod]
        public static void ConfigureServices(IServiceCollection services, TaskUiOptions options)
        {
            services.Configure<TaskUiOptions>(o => o.UseTenantSupport = options.UseTenantSupport);
        }

        [MvcRegistrationMethod]
        public static void EnableTaskViews(ApplicationPartManager manager, object cfg)
        {
            manager.EnableTaskSchedulerViews();
        }
    }
}
