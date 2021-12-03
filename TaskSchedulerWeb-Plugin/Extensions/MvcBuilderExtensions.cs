using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;

namespace TaskSchedulerWeb.Extensions
{
    public static class MvcBuilderExtensions
    {
        public static ApplicationPartManager EnableTaskSchedulerViews(this ApplicationPartManager manager)
        {
            ApplicationPart part = new CompiledRazorAssemblyPart(typeof(MvcBuilderExtensions).Assembly);
            
            manager.ApplicationParts.Add(part);
            return manager;
        }
    }
}
