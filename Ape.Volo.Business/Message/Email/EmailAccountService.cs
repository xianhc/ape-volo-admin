using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Model;
using Ape.Volo.Core;
using Ape.Volo.Core.Utils;
using Ape.Volo.Entity.Core.Message.Email;
using Ape.Volo.IBusiness.Message.Email;
using Ape.Volo.SharedModel.Dto.Core.Message.Email;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.Message;
using Ape.Volo.ViewModel.Core.Message.Email;
using Ape.Volo.ViewModel.Report.Message.Email.Account;

namespace Ape.Volo.Business.Message.Email;

/// <summary>
/// 邮箱账户服务
/// </summary>
public class EmailAccountService : BaseServices<EmailAccount>, IEmailAccountService
{
    #region 基础方法

    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="createUpdateEmailAccountDto"></param>
    /// <returns></returns>
    public async Task<OperateResult> CreateAsync(CreateUpdateEmailAccountDto createUpdateEmailAccountDto)
    {
        if (await TableWhere(x => x.Email == createUpdateEmailAccountDto.Email).AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateEmailAccountDto,
                nameof(createUpdateEmailAccountDto.Email)));
        }

        var emailAccount = App.Mapper.MapTo<EmailAccount>(createUpdateEmailAccountDto);
        var result = await AddAsync(emailAccount);
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateEmailAccountDto"></param>
    /// <returns></returns>
    public async Task<OperateResult> UpdateAsync(CreateUpdateEmailAccountDto createUpdateEmailAccountDto)
    {
        var oldEmailAccount = await TableWhere(x => x.Id == createUpdateEmailAccountDto.Id).FirstAsync();
        if (oldEmailAccount.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist(createUpdateEmailAccountDto,
                LanguageKeyConstants.EmailAccount,
                nameof(createUpdateEmailAccountDto.Id)));
        }

        if (oldEmailAccount.Email != createUpdateEmailAccountDto.Email &&
            await TableWhere(j => j.Email == createUpdateEmailAccountDto.Email).AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateEmailAccountDto,
                nameof(createUpdateEmailAccountDto.Email)));
        }

        var emailAccount = App.Mapper.MapTo<EmailAccount>(createUpdateEmailAccountDto);
        var result = await UpdateAsync(emailAccount);
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public async Task<OperateResult> DeleteAsync(HashSet<long> ids)
    {
        var emailAccounts = await TableWhere(x => ids.Contains(x.Id)).ToListAsync();
        if (emailAccounts.Count < 1)
        {
            return OperateResult.Error(ValidationError.NotExist());
        }

        var result = await LogicDelete<EmailAccount>(x => ids.Contains(x.Id));
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="emailAccountQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    public async Task<List<EmailAccountVo>> QueryAsync(EmailAccountQueryCriteria emailAccountQueryCriteria,
        Pagination pagination)
    {
        var queryOptions = new QueryOptions<EmailAccount>
        {
            Pagination = pagination,
            ConditionalModels = emailAccountQueryCriteria.ApplyQueryConditionalModel(),
        };
        return App.Mapper.MapTo<List<EmailAccountVo>>(
            await TablePageAsync(queryOptions));
    }

    /// <summary>
    /// 查询所有
    /// </summary>
    /// <returns></returns>
    public async Task<List<EmailAccountVo>> QueryAllAsync()
    {
        return App.Mapper.MapTo<List<EmailAccountVo>>(await Table.ToListAsync());
    }


    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="emailAccountQueryCriteria"></param>
    /// <returns></returns>
    public async Task<List<ExportBase>> DownloadAsync(EmailAccountQueryCriteria emailAccountQueryCriteria)
    {
        var emailAccounts = await TableWhere(emailAccountQueryCriteria.ApplyQueryConditionalModel()).ToListAsync();
        List<ExportBase> emailAccountExports = new List<ExportBase>();
        emailAccountExports.AddRange(emailAccounts.Select(x => new EmailAccountExport
        {
            Id = x.Id,
            Email = x.Email,
            DisplayName = x.DisplayName,
            Host = x.Host,
            Port = x.Port,
            UserName = x.UserName,
            EnableSsl = x.EnableSsl,
            UseDefaultCredentials = x.UseDefaultCredentials,
            CreateTime = x.CreateTime
        }));
        return emailAccountExports;
    }

    #endregion
}
