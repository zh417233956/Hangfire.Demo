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

            var jobId = Hangfire.BackgroundJob.Schedule<TaskService>(t=>t.TestMethod(DateTime.Now), TimeSpan.FromMinutes(1));

            return jobId;
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