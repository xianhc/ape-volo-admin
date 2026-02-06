using System.Collections.Generic;
using System.Threading.Tasks;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.Core.Message.Email;
using Ape.Volo.SharedModel.Dto.Core.Message.Email;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.Message;
using Ape.Volo.ViewModel.Core.Message.Email;

namespace Ape.Volo.IBusiness.Message.Email;

/// <summary>
/// 邮箱账户接口
/// </summary>
public interface IEmailAccountService : IBaseServices<EmailAccount>
{
    #region 基础接口

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateEmailAccountDto"></param>
    /// <returns></returns>
    Task<OperateResult> CreateAsync(CreateUpdateEmailAccountDto createUpdateEmailAccountDto);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateEmailAccountDto"></param>
    /// <returns></returns>
    Task<OperateResult> UpdateAsync(CreateUpdateEmailAccountDto createUpdateEmailAccountDto);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    Task<OperateResult> DeleteAsync(HashSet<long> ids);

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="emailAccountQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    Task<List<EmailAccountVo>> QueryAsync(EmailAccountQueryCriteria emailAccountQueryCriteria,
        Pagination pagination);

    /// <summary>
    /// 查询
    /// </summary>
    /// <returns></returns>
    Task<List<EmailAccountVo>> QueryAllAsync();


    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="emailAccountQueryCriteria"></param>
    /// <returns></returns>
    Task<List<ExportBase>> DownloadAsync(EmailAccountQueryCriteria emailAccountQueryCriteria);

    #endregion
}
