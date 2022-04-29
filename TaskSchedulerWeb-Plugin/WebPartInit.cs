using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITVComponents.WebCoreToolkit.AspExtensions;
using ITVComponents.WebCoreToolkit.AspExtensions.Impl;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using TaskSchedulerWeb.Extensions;

namespace TaskSchedulerWeb
{
    [WebPart]
    public static class WebPartInit
    {
        [MvcRegistrationMethod]
        public static void EnableTaskViews(ApplicationPartManager manager, object cfg)
        {
            manager.EnableTaskSchedulerViews();
        }
    }
}
