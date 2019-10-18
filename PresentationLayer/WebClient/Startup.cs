using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace WebClient
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

            services.AddTransient<API.TaskService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            loggerFactory.AddLog4Net("Configs/log4net.config");

            string HangfireServer = Configuration.GetConnectionString("Hangfire.Server");

            app.UseHangfireServer();

            if (HangfireServer == "True")
            {
                app.UseHangfireDashboard("/hangfire", new DashboardOptions()
                {
                    Authorization = new[] { new Filters.HangfireDashboardAuthorizeFilter() }
                });
            }

            app.UseMvc();

        }
    }
}
