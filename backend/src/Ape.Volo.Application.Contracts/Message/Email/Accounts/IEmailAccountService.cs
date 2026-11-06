using Ape.Volo.Application.Contracts.Message.Email.Accounts.Models;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.Message.Email.Accounts;

/// <summary>
/// 邮箱账户接口
/// </summary>
public interface IEmailAccountService : IApplicationService
{
    #region 基础接口

    /// <summary>
    /// 按主键查询单条
    /// </summary>
    /// <param name="id">主键</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>邮箱账户，不存在时返回 null</returns>
    // 仅供进程内 IEmailAccountLookupService 读取，不对外暴露。
    [RemoteService(false)]
    Task<EmailAccountVo?> GetByIdAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateEmailAccountDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> CreateAsync(CreateUpdateEmailAccountDto createUpdateEmailAccountDto, CancellationToken ct = default);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateEmailAccountDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> UpdateAsync(CreateUpdateEmailAccountDto createUpdateEmailAccountDto, CancellationToken ct = default);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> DeleteAsync(HashSet<long> ids, CancellationToken ct = default);

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="emailAccountQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<PagedResultDto<EmailAccountVo>> GetListAsync(EmailAccountQueryCriteria emailAccountQueryCriteria, CancellationToken ct = default);

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<EmailAccountVo>> GetAllAsync(CancellationToken ct = default);

    #endregion
}
