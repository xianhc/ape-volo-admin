using System.Reflection;
using Ape.Volo.Application.Contracts.Abstractions;
using Ape.Volo.Core.ConfigOptions;
using Ape.Volo.Core.Repository.UnitOfWork;
using Ape.Volo.Entity.Attributes;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace Ape.Volo.Core.Repository.SugarHandler;

/// <summary>
/// SqlSugar仓储
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class SugarRepository<TEntity> : ISugarRepository<TEntity> where TEntity : class, new()
{
    /// <summary>
    /// 初始化 <see cref="SugarRepository"/> 的新实例。
    /// </summary>
    /// <param name="unitOfWork">unitOfWork</param>
    /// <param name="logDataConnectionOptions">日志库连接配置</param>
    /// <param name="currentUser">当前请求身份，供审计字段使用</param>
    public SugarRepository(IUnitOfWork unitOfWork, IOptions<LogDataConnectionOptions> logDataConnectionOptions,
        ICurrentUser currentUser)
    {
        CurrentUserId = currentUser.Id > 0 ? currentUser.Id : null;
        var sqlSugarScope = unitOfWork.GetDbClient();
        var logDbAttribute = typeof(TEntity).GetCustomAttribute<LogDataBaseAttribute>();
        if (logDbAttribute != null)
        {
            SugarClient = sqlSugarScope.GetConnectionScope(logDataConnectionOptions.Value.ConnId);
            return;
        }

        SugarClient = sqlSugarScope;
    }

    public ISqlSugarClient SugarClient { get; }

    /// <inheritdoc />
    public long? CurrentUserId { get; }
}
