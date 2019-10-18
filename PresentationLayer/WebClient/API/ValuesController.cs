using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebClient.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        ILogger<ValuesController> logger;
        public ValuesController(ILogger<ValuesController> logger)
        {
            this.logger = logger;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            logger.LogWarning("开始设置延迟作业,1分钟以后执行");

            //创建延迟作业
            var jobId = Hangfire.BackgroundJob.Schedule<TaskService>(t => t.TestMethod(DateTime.Now), TimeSpan.FromMinutes(1));

            #region 查找周期性作业是否存在，不存在则创建作业

            var list = Hangfire.JobStorage.Current.GetConnection().GetAllEntriesFromHash("recurring-job:Client_Recurring");
            if (list == null)
            {
                logger.LogWarning("未找到周期作业,创建1分钟周期性作业");
                Hangfire.RecurringJob.AddOrUpdate<TaskService>("Client_Recurring", t => t.TestMethod(DateTime.Now), Hangfire.Cron.Minutely, TimeZoneInfo.Local);
            }
            else
            {
                logger.LogWarning("已存在周期性作业");
            }
            #endregion 查找周期性作业是否存在，不存在则创建作业

            return "OK";
        }
    }
    public class TaskService
    {
        ILogger<TaskService> logger;
        public TaskService(ILogger<TaskService> logger)
        {
            this.logger = logger;
        }
        public void TestMethod(DateTime datetime)
        {
            //记录日志
            logger.LogWarning($"创建时间:{datetime.ToString()}");
        }
    }
}