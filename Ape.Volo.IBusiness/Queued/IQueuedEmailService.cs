using System.Collections.Generic;
using System.Threading.Tasks;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.Core.Queued;
using Ape.Volo.SharedModel.Dto.Core.Queued;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.Queued;
using Ape.Volo.ViewModel.Core.Queued;

namespace Ape.Volo.IBusiness.Queued;

/// <summary>
/// 邮件队列接口
/// </summary>
public interface IQueuedEmailService : IBaseServices<QueuedEmail>
{
    #region 基础接口

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateQueuedEmailDto"></param>
    /// <returns></returns>
    Task<OperateResult> CreateAsync(CreateUpdateQueuedEmailDto createUpdateQueuedEmailDto);

    /// <summary>
    /// 更新发送次数
    /// </summary>
    /// <param name="queuedEmail"></param>
    /// <returns></returns>
    Task<OperateResult> UpdateTriesAsync(QueuedEmail queuedEmail);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateQueuedEmailDto"></param>
    /// <returns></returns>
    Task<OperateResult> UpdateAsync(CreateUpdateQueuedEmailDto createUpdateQueuedEmailDto);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    Task<OperateResult> DeleteAsync(HashSet<long> ids);

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="queuedEmailQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    Task<List<QueuedEmailVo>> QueryAsync(QueuedEmailQueryCriteria queuedEmailQueryCriteria, Pagination pagination);

    #endregion

    #region 扩展接口

    /// <summary>
    /// 重置邮箱
    /// </summary>
    /// <param name="emailAddress"></param>
    /// <param name="messageTemplateName"></param>
    /// <returns></returns>
    Task<OperateResult> ResetEmailCode(string emailAddress, string messageTemplateName);


    /// <summary>
    /// 查询 发送邮件
    /// </summary>
    /// <param name="queuedEmailQueryCriteria"></param>
    /// <returns></returns>
    Task<List<QueuedEmail>> QueryToSendMailAsync(QueuedEmailQueryCriteria queuedEmailQueryCriteria);

    #endregion
}
