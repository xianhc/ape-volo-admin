using Ape.Volo.Application.Contracts.Message.Email.Accounts.Models;

namespace Ape.Volo.Application.Contracts.Message.Email.Accounts;

/// <summary>
/// 邮件编排和后台发送读取账户的内部端口，不生成远程接口。
/// </summary>
public interface IEmailAccountLookupService
{
    /// <summary>
    /// 读取发送邮件所需的账户配置。
    /// </summary>
    /// <param name="id">账户 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>账户，不存在时为 null</returns>
    Task<EmailAccountVo?> FindByIdAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// 批量读取发送邮件所需的账户配置。
    /// </summary>
    /// <param name="ids">账户 ID 集合</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>按账户 ID 索引的账户配置</returns>
    Task<IReadOnlyDictionary<long, EmailAccountVo>> FindByIdsAsync(
        IReadOnlyCollection<long> ids, CancellationToken ct = default);
}
