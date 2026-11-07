using Ape.Volo.Application.Contracts.Authorization;
using Ape.Volo.Application.Contracts.System.Dictionaries;
using Ape.Volo.Application.Contracts.System.Dictionaries.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Core.Repository.UnitOfWork;
using Ape.Volo.Entity.System.Dictionaries;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.System.Dictionaries;

/// <summary>
/// 字典详情服务
/// </summary>
public class DictDetailService : BaseService<DictDetail>, IDictDetailService
{
    private readonly ICache _cache;
    private readonly IUnitOfWork _unitOfWork;
    #region 构造函数

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sugarRepository">当前实体的仓储</param>
    /// <param name="mapper">对象映射器</param>
    /// <param name="cache">缓存存储。</param>
    /// <param name="unitOfWork">提交后失效登记。</param>
    public DictDetailService(ISugarRepository<DictDetail> sugarRepository, IMapper mapper, ICache cache, IUnitOfWork unitOfWork) : base(sugarRepository, mapper)
    {
        _cache = cache;
        _unitOfWork = unitOfWork;
    }

    #endregion

    #region 基础方法

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Dictionaries.Add)]
    [UseTran]
    public async Task<OperateResult> CreateAsync(CreateUpdateDictDetailDto createUpdateDictDetailDto, CancellationToken ct = default)
    {
        if (
            await TableWhere(x =>
                x.DictId == createUpdateDictDetailDto.DictId && x.Label == createUpdateDictDetailDto.Label).AnyAsync(ct))
        {
            return AlreadyExists(() => createUpdateDictDetailDto.Label);
        }

        if (await TableWhere(x =>
                x.DictId == createUpdateDictDetailDto.DictId && x.Value == createUpdateDictDetailDto.Value).AnyAsync(ct))
        {
            return AlreadyExists(() => createUpdateDictDetailDto.Value);
        }

        var dictDetail = Mapper.MapTo<DictDetail>(createUpdateDictDetailDto);
        var result = await AddAsync(dictDetail, ct: ct);
        if (result) await InvalidateAsync([dictDetail.DictId], ct);
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Dictionaries.Edit)]
    [UseTran]
    public async Task<OperateResult> UpdateAsync(CreateUpdateDictDetailDto createUpdateDictDetailDto, CancellationToken ct = default)
    {
        var oldDictDetail =
            await TableWhere(x => x.Id == createUpdateDictDetailDto.Id).FirstAsync(ct);

        if (oldDictDetail.IsNull())
        {
            return NotFound(createUpdateDictDetailDto.Id);
        }

        if (oldDictDetail.Label != createUpdateDictDetailDto.Label &&
            await TableWhere(x =>
                x.DictId == createUpdateDictDetailDto.DictId && x.Label == createUpdateDictDetailDto.Label).AnyAsync(ct))
        {
            return AlreadyExists(() => createUpdateDictDetailDto.Label);
        }

        if (oldDictDetail.Value != createUpdateDictDetailDto.Value &&
            await TableWhere(x =>
                x.DictId == createUpdateDictDetailDto.DictId && x.Value == createUpdateDictDetailDto.Value).AnyAsync(ct))
        {
            return AlreadyExists(() => createUpdateDictDetailDto.Value);
        }


        var dictDetail = Mapper.MapTo<DictDetail>(createUpdateDictDetailDto);
        var result = await UpdateAsync(dictDetail, ct: ct);
        if (result) await InvalidateAsync([oldDictDetail.DictId, dictDetail.DictId], ct);
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Dictionaries.Delete)]
    [UseTran]
    public async Task<OperateResult> DeleteAsync(HashSet<long> ids, CancellationToken ct = default)
    {
        var dictList = await TableWhere(x => ids.Contains(x.Id)).ToListAsync(ct);
        if (dictList.Count <= 0)
        {
            return NotFound();
        }

        var result = await LogicDeleteAsync(x => ids.Contains(x.Id), ct);
        if (result) await InvalidateAsync(dictList.Select(item => item.DictId), ct);
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Dictionaries.List)]
    public Task<PagedResultDto<DictDetailVo>> GetListAsync(DictDetailQueryCriteria dictDetailQueryCriteria, CancellationToken ct = default)
        => PageAsync<DictDetailVo>(dictDetailQueryCriteria, ct: ct);

    private async Task InvalidateAsync(IEnumerable<long> dictIds, CancellationToken ct)
    {
        var ids = dictIds.Distinct().ToArray();
        var names = await SugarClient.Queryable<Dict>().Where(dict => ids.Contains(dict.Id))
            .Select(dict => dict.Name).ToListAsync(ct);
        await _unitOfWork.AfterCommitAsync(async () =>
        {
            foreach (var name in names)
                await _cache.RemoveAsync(GlobalConstants.CachePrefix.LoadDictByName + name);
        });
    }

    #endregion

}
