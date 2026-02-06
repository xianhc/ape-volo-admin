using Ape.Volo.Common.Attributes.Redis;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Helper;
using Ape.Volo.Core.Caches.Redis.MessageQueue;
using Ape.Volo.Entity.Log;
using Ape.Volo.IBusiness.Log;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Ape.Volo.Infrastructure.Messaging.Redis;

public class AuditLogSubscribe : IRedisSubscribe
{
    #region Fields

    private readonly ILogger<AuditLogSubscribe> _logger;
    private readonly IOperateLogService _operateLogService;

    #endregion

    #region Ctor

    public AuditLogSubscribe(IOperateLogService operateLogService, ILogger<AuditLogSubscribe> logger)
    {
        _operateLogService = operateLogService;
        _logger = logger;
    }

    #endregion

    [SubscribeDelay(MqTopicNameKey.OperateLogQueue, true)]
    private async Task DoSub(List<RedisValue> redisValues)
    {
        try
        {
            if (redisValues.Any())
            {
                List<OperateLog> operateLogs = new List<OperateLog>();
                redisValues.ForEach(x => { operateLogs.Add(x.ToString().ToObject<OperateLog>()); });
                await _operateLogService.CreateListAsync(operateLogs);
            }
        }
        catch (Exception e)
        {
            _logger.LogCritical(ExceptionHelper.GetExceptionAllMsg(e));
        }
    }
}