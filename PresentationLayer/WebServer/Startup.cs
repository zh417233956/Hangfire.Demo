using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using WebServer.Filters;

namespace WebServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            #region Hangfire配置
            string HangfireCon = Configuration.GetConnectionString("Hangfire.Redis");
            var RedisOptions = ConfigurationOptions.Parse(HangfireCon);
            var HangfireRedis = ConnectionMultiplexer.Connect(RedisOptions);

            services.AddHangfire(configuration =>
            {
                configuration.UseRedisStorage(HangfireRedis, new Hangfire.Redis.RedisStorageOptions() { Db = (int)RedisOptions.DefaultDatabase, Prefix = "{hangfire}:Mr.right:" });
            });
            #endregion Hangfire配置

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.Run(async (context) =>
            //{
            //    await context.Response.WriteAsync("Hello World!");
            //});

            app.UseHangfireServer();

            app.UseHangfireDashboard("/hangfire", new DashboardOptions()
            {
                Authorization = new[] { new HangfireDashboardAuthorizeFilter() }
            });

            app.UseMvc();
        }
    }
}
