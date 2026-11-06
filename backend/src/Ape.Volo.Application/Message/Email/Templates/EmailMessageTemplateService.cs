using Ape.Volo.Application.Contracts.Authorization;
using Ape.Volo.Application.Contracts.Message.Email.Templates.Models;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Shared.Model;
using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Core.Utils;
using Ape.Volo.Entity.Message.Email;

namespace Ape.Volo.Application.Message.Email.Templates;

/// <summary>
/// 邮件消息模板实现
/// </summary>
public class EmailMessageTemplateService : BaseService<EmailMessageTemplate>, IEmailMessageTemplateService
{
    #region 构造函数

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sugarRepository">当前实体的仓储</param>
    /// <param name="mapper">对象映射器</param>
    public EmailMessageTemplateService(ISugarRepository<EmailMessageTemplate> sugarRepository, IMapper mapper) : base(sugarRepository, mapper)
    {
    }

    #endregion

    #region 基础方法

    /// <inheritdoc />
    [HasPerm(SystemPermissions.EmailTemplates.Add)]
    public async Task<OperateResult> CreateAsync(
        CreateUpdateEmailMessageTemplateDto createUpdateEmailMessageTemplateDto, CancellationToken ct = default)
    {
        if (await TableWhere(x => x.Name == createUpdateEmailMessageTemplateDto.Name).AnyAsync(ct))
        {
            return AlreadyExists(() => createUpdateEmailMessageTemplateDto.Name);
        }


        var result = await AddAsync(Mapper.MapTo<EmailMessageTemplate>(createUpdateEmailMessageTemplateDto), ct: ct);
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.EmailTemplates.Edit)]
    public async Task<OperateResult> UpdateAsync(
        CreateUpdateEmailMessageTemplateDto createUpdateEmailMessageTemplateDto, CancellationToken ct = default)
    {
        var emailMessageTemplate = await TableWhere(x => x.Id == createUpdateEmailMessageTemplateDto.Id).FirstAsync(ct);
        if (emailMessageTemplate.IsNull())
        {
            return NotFound(createUpdateEmailMessageTemplateDto.Id);
        }

        if (emailMessageTemplate.Name != createUpdateEmailMessageTemplateDto.Name &&
            await TableWhere(j => j.Name == emailMessageTemplate.Name).AnyAsync(ct))
        {
            return AlreadyExists(() => createUpdateEmailMessageTemplateDto.Name);
        }

        var result = await UpdateAsync(
            Mapper.MapTo<EmailMessageTemplate>(createUpdateEmailMessageTemplateDto), ct: ct);
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.EmailTemplates.Delete)]
    public async Task<OperateResult> DeleteAsync(HashSet<long> ids, CancellationToken ct = default)
    {
        var messageTemplateList = await TableWhere(x => ids.Contains(x.Id)).ToListAsync(ct);
        if (messageTemplateList.Count <= 0)
        {
            return NotFound();
        }

        var result = await LogicDeleteAsync(x => ids.Contains(x.Id), ct);
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.EmailTemplates.List)]
    public Task<PagedResultDto<EmailMessageTemplateVo>> GetListAsync(
        EmailMessageTemplateQueryCriteria messageTemplateQueryCriteria, CancellationToken ct = default)
        => PageAsync<EmailMessageTemplateVo>(messageTemplateQueryCriteria, ct: ct);

    /// <inheritdoc />
    public async Task<EmailMessageTemplateVo?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        var template = await TableWhere(x => x.Name == name).FirstAsync(ct);
        return template == null ? null : Mapper.MapTo<EmailMessageTemplateVo>(template);
    }

    #endregion

}
