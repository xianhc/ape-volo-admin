using Ape.Volo.Application.Contracts.Log.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Log;

[assembly: AutoMapping(typeof(ExceptionLog), typeof(ExceptionLogVo))]
[assembly: AutoMapping(typeof(OperateLog), typeof(OperateLogVo))]
[assembly: AutoMapping(typeof(ScheduledJobLog), typeof(ScheduledJobLogVo))]
