using Ape.Volo.Application.Contracts.Message.Email.QueuedEmails.Models;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.Message.Email.QueuedEmails;

/// <summary>
/// 邮件队列接口
/// </summary>
public interface IQueuedEmailService : IApplicationService
{
    #region 基础接口

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateQueuedEmailDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> CreateAsync(CreateUpdateQueuedEmailDto createUpdateQueuedEmailDto, CancellationToken ct = default);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateQueuedEmailDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> UpdateAsync(CreateUpdateQueuedEmailDto createUpdateQueuedEmailDto, CancellationToken ct = default);

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
    /// <param name="queuedEmailQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<PagedResultDto<QueuedEmailVo>> GetListAsync(QueuedEmailQueryCriteria queuedEmailQueryCriteria, CancellationToken ct = default);

    #endregion

    #region 扩展接口

    /// <summary>
    /// 重置邮箱
    /// </summary>
    /// <param name="emailAddress"></param>
    /// <param name="messageTemplateName"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    [RemoteService(false)]
    Task<OperateResult> ResetEmailCode(string emailAddress, string messageTemplateName, CancellationToken ct = default);


    /// <summary>
    /// 发送所有尚未达到重试上限的队列邮件。
    /// </summary>
    /// <param name="ct">取消令牌</param>
    [RemoteService(false)]
    Task SendPendingAsync(CancellationToken ct = default);

    #endregion
}
