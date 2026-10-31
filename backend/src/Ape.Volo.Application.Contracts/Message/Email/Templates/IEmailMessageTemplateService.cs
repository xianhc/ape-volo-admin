using Ape.Volo.Application.Contracts.Message.Email.Templates.Models;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.Message.Email.Templates;

/// <summary>
/// 邮件消息模板接口
/// </summary>
public interface IEmailMessageTemplateService : IApplicationService
{
    #region 基础接口

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateEmailMessageTemplateDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> CreateAsync(CreateUpdateEmailMessageTemplateDto createUpdateEmailMessageTemplateDto, CancellationToken ct = default);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateEmailMessageTemplateDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> UpdateAsync(CreateUpdateEmailMessageTemplateDto createUpdateEmailMessageTemplateDto, CancellationToken ct = default);

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
    /// <param name="messageTemplateQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<PagedResultDto<EmailMessageTemplateVo>> GetListAsync(EmailMessageTemplateQueryCriteria messageTemplateQueryCriteria, CancellationToken ct = default);

    /// <summary>
    /// 按模板名称查询
    /// </summary>
    /// <param name="name">模板名称</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>模板，不存在时返回 null</returns>
    [RemoteService(false)]
    Task<EmailMessageTemplateVo?> QueryByNameAsync(string name, CancellationToken ct = default);

    #endregion
}
