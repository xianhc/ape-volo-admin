using Ape.Volo.Application.Contracts.Authorization;
using Ape.Volo.Application.Contracts.Log.Models;
using Ape.Volo.Shared.Model;
using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Entity.Log;

namespace Ape.Volo.Application.Log;

/// <summary>
/// 系统日志服务
/// </summary>
public class ExceptionLogService : BaseService<ExceptionLog>, IExceptionLogService
{
    #region 构造函数

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sugarRepository">当前实体的仓储</param>
    /// <param name="mapper">对象映射器</param>
    public ExceptionLogService(ISugarRepository<ExceptionLog> sugarRepository, IMapper mapper) : base(sugarRepository, mapper)
    {
    }

    #endregion

    #region 基础方法

    /// <inheritdoc />
    [HasPerm(SystemPermissions.ExceptionLogs.List)]
    public Task<PagedResultDto<ExceptionLogVo>> GetListAsync(LogQueryCriteria logQueryCriteria, CancellationToken ct = default)
        => PageAsync<ExceptionLogVo>(logQueryCriteria, options => options.IsSplitTable = true, ct);

    #endregion

}
