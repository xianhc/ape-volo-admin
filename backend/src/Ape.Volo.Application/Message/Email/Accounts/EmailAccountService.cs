using Ape.Volo.Application.Contracts.Authorization;
using Ape.Volo.Application.Contracts.Message.Email.Accounts.Models;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Shared.Model;
using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Core.Utils;
using Ape.Volo.Entity.Message.Email;

namespace Ape.Volo.Application.Message.Email.Accounts;

/// <summary>
/// 邮箱账户服务
/// </summary>
public class EmailAccountService : BaseService<EmailAccount>, IEmailAccountService, IEmailAccountLookupService
{
    #region 构造函数

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sugarRepository">当前实体的仓储</param>
    /// <param name="mapper">对象映射器</param>
    public EmailAccountService(ISugarRepository<EmailAccount> sugarRepository, IMapper mapper) : base(sugarRepository, mapper)
    {
    }

    #endregion

    #region 基础方法

    /// <inheritdoc />
    Task<EmailAccountVo?> IEmailAccountLookupService.FindByIdAsync(long id, CancellationToken ct) => QueryByIdAsync(id, ct);

    /// <inheritdoc />
    async Task<IReadOnlyDictionary<long, EmailAccountVo>> IEmailAccountLookupService.FindByIdsAsync(
        IReadOnlyCollection<long> ids, CancellationToken ct)
    {
        if (ids.Count == 0)
        {
            return new Dictionary<long, EmailAccountVo>();
        }

        var accounts = await TableWhere(x => ids.Contains(x.Id)).ToListAsync(ct);
        return Mapper.MapTo<List<EmailAccountVo>>(accounts).ToDictionary(x => x.Id);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.EmailAccounts.List)]
    public async Task<EmailAccountVo?> QueryByIdAsync(long id, CancellationToken ct = default)
    {
        var emailAccount = await GetByIdAsync(id, ct);
        return emailAccount == null ? null : Mapper.MapTo<EmailAccountVo>(emailAccount);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.EmailAccounts.Add)]
    public async Task<OperateResult> CreateAsync(CreateUpdateEmailAccountDto createUpdateEmailAccountDto, CancellationToken ct = default)
    {
        if (await TableWhere(x => x.Email == createUpdateEmailAccountDto.Email).AnyAsync(ct))
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateEmailAccountDto,
                nameof(createUpdateEmailAccountDto.Email)));
        }

        var emailAccount = Mapper.MapTo<EmailAccount>(createUpdateEmailAccountDto);
        var result = await AddAsync(emailAccount, ct: ct);
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.EmailAccounts.Edit)]
    public async Task<OperateResult> UpdateAsync(CreateUpdateEmailAccountDto createUpdateEmailAccountDto, CancellationToken ct = default)
    {
        var oldEmailAccount = await TableWhere(x => x.Id == createUpdateEmailAccountDto.Id).FirstAsync(ct);
        if (oldEmailAccount.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist(createUpdateEmailAccountDto,
                "邮件账户",
                nameof(createUpdateEmailAccountDto.Id)));
        }

        if (oldEmailAccount.Email != createUpdateEmailAccountDto.Email &&
            await TableWhere(j => j.Email == createUpdateEmailAccountDto.Email).AnyAsync(ct))
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateEmailAccountDto,
                nameof(createUpdateEmailAccountDto.Email)));
        }

        var emailAccount = Mapper.MapTo<EmailAccount>(createUpdateEmailAccountDto);
        var result = await UpdateAsync(emailAccount, ct: ct);
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.EmailAccounts.Delete)]
    public async Task<OperateResult> DeleteAsync(HashSet<long> ids, CancellationToken ct = default)
    {
        var emailAccounts = await TableWhere(x => ids.Contains(x.Id)).ToListAsync(ct);
        if (emailAccounts.Count < 1)
        {
            return OperateResult.Error(ValidationError.NotExist());
        }

        var result = await LogicDeleteAsync(x => ids.Contains(x.Id), ct);
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.EmailAccounts.List)]
    public Task<PagedResultDto<EmailAccountVo>> GetListAsync(EmailAccountQueryCriteria emailAccountQueryCriteria, CancellationToken ct = default)
        => PageAsync<EmailAccountVo>(emailAccountQueryCriteria, ct: ct);

    /// <inheritdoc />
    [HasPerm(SystemPermissions.EmailAccounts.List, SystemPermissions.EmailTemplates.List, SystemPermissions.EmailTemplates.Add, SystemPermissions.EmailTemplates.Edit, SystemPermissions.QueuedEmails.List, SystemPermissions.QueuedEmails.Add, SystemPermissions.QueuedEmails.Edit)]
    public async Task<List<EmailAccountVo>> GetAllAsync(CancellationToken ct = default)
    {
        return Mapper.MapTo<List<EmailAccountVo>>(await Table.ToListAsync(ct));
    }


    /// <inheritdoc />
    [HasPerm(SystemPermissions.EmailAccounts.Download)]
    public async Task<List<ExportBase>> DownloadAsync(EmailAccountQueryCriteria emailAccountQueryCriteria, CancellationToken ct = default)
    {
        var emailAccounts = await TableWhere(emailAccountQueryCriteria.ApplyQueryConditionalModel()).ToListAsync(ct);
        List<ExportBase> emailAccountExports = new List<ExportBase>();
        emailAccountExports.AddRange(Mapper.MapTo<List<EmailAccountExport>>(emailAccounts));
        return emailAccountExports;
    }

    #endregion

}
