using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITVComponents.EFRepo.DIIntegration;
using ITVComponents.EFRepo.Options;
using ITVComponents.Plugins.Initialization;
using ITVComponents.WebCoreToolkit.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;

namespace PeriodicTasks.DbContext.Filtering
{
    public class ContextFilterInitializer<TContext>: DbModelBuilderOptionsProvider<TContext> where TContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public ContextFilterInitializer() : base()
        {

        }
        public ContextFilterInitializer(DbModelBuilderOptionsProvider<TContext> innerBuilder):base(innerBuilder) { }

        protected override void Configure(DbContextModelBuilderOptions<TContext> options)
        {
            FilterPreparation.ConfigureFilters<TContext>(options);
        }
    }
}
