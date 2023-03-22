using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITVComponents.EFRepo.DbContextConfig.Expressions;
using ITVComponents.EFRepo.Options;
using ITVComponents.Plugins.DatabaseDrivenConfiguration.Models;
using ITVComponents.WebCoreToolkit.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using PeriodicTasks.DatabaseDrivenTaskLoading.Models;

namespace PeriodicTasks.DbContext.Filtering
{
    public static class FilterPreparation
    {
        public static void ConfigureFilters<TContext>(DbContextModelBuilderOptions<TContext> options)
        {
            options.ConfigureGlobalFilter<PeriodicLog>(n => n.TenantId == CurrentTenant);
            options.ConfigureGlobalFilter<PeriodicMonthday>(n => n.TenantId == CurrentTenant);
            options.ConfigureGlobalFilter<PeriodicMonth>(n => n.TenantId == CurrentTenant);
            options.ConfigureGlobalFilter<PeriodicRun>(n => n.TenantId == CurrentTenant);
            options.ConfigureGlobalFilter<PeriodicSchedule>(n => n.TenantId == CurrentTenant);
            options.ConfigureGlobalFilter<PeriodicStepParameter>(n => n.TenantId == CurrentTenant);
            options.ConfigureGlobalFilter<PeriodicStep>(n => n.TenantId == CurrentTenant);
            options.ConfigureGlobalFilter<DatabaseDrivenTaskLoading.Models.PeriodicTask>(n => n.TenantId == CurrentTenant);
            options.ConfigureGlobalFilter<PeriodicTime>(n => n.TenantId == CurrentTenant);
            options.ConfigureGlobalFilter<PeriodicWeekday>(n => n.TenantId == CurrentTenant);

        }

        [ExpressionPropertyRedirect("CurrentTenant")]
        private static string CurrentTenant => "";
    }
}
