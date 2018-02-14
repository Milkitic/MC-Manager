using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using gm.Functions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace gm
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddDistributedMemoryCache();
            services.AddSession();
            //services.Add(new ServiceDescriptor(typeof(DbHelper), new DbHelper(Configuration.GetConnectionString("DefaultConnection"))));
            DataBase.DbHelper = new DbHelper(Configuration.GetConnectionString("DefaultConnection"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseStatusCodePagesWithReExecute("/errors/{0}");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseSession();
            //Microsoft.AspNetCore.Http.HttpContext.RequestServices.GetService(typeof(DbHelper)) as DbHelper;
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Front}/{action=Index}/{id?}");
            });
        }
    }
}
