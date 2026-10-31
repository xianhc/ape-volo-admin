using Microsoft.AspNetCore.Authorization;
using Ape.Volo.Application.Contracts.Authorization;
using Ape.Volo.Application.Contracts.SystemManagement.Dictionaries.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Shared.Model;
using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Core.Repository.UnitOfWork;
using Ape.Volo.Core.Utils;
using Ape.Volo.Entity.SystemManagement.Dictionaries;

namespace Ape.Volo.Application.SystemManagement.Dictionaries;

/// <summary>
/// 字典服务
/// </summary>
public class DictService : BaseService<Dict>, IDictService
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
    public DictService(ISugarRepository<Dict> sugarRepository, IMapper mapper, ICache cache, IUnitOfWork unitOfWork) : base(sugarRepository, mapper)
    {
        _cache = cache;
        _unitOfWork = unitOfWork;
    }

    #endregion

    #region 基础方法

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Dictionaries.Add)]
    [UseTran]
    public async Task<OperateResult> CreateAsync(CreateUpdateDictDto createUpdateDictDto, CancellationToken ct = default)
    {
        if (await TableWhere(d => d.Name == createUpdateDictDto.Name).AnyAsync(ct))
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateDictDto, nameof(createUpdateDictDto.Name)));
        }

        var result = await AddAsync(Mapper.MapTo<Dict>(createUpdateDictDto), ct: ct);
        if (result) await InvalidateAsync(createUpdateDictDto.Name);
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Dictionaries.Edit)]
    [UseTran]
    public async Task<OperateResult> UpdateAsync(CreateUpdateDictDto createUpdateDictDto, CancellationToken ct = default)
    {
        var oldDict =
            await TableWhere(x => x.Id == createUpdateDictDto.Id).FirstAsync(ct);
        if (oldDict.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist(createUpdateDictDto, "字典",
                nameof(createUpdateDictDto.Id)));
        }

        if (oldDict.Name != createUpdateDictDto.Name &&
            await TableWhere(j => j.Name == createUpdateDictDto.Name).AnyAsync(ct))
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateDictDto, nameof(createUpdateDictDto.Name)));
        }

        var result = await UpdateAsync(Mapper.MapTo<Dict>(createUpdateDictDto), ct: ct);
        if (result)
        {
            await InvalidateAsync(oldDict.Name);
            await InvalidateAsync(createUpdateDictDto.Name);
        }
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
            return OperateResult.Error(ValidationError.NotExist());
        }

        var result = await LogicDeleteAsync(x => ids.Contains(x.Id), ct);
        if (result)
            foreach (var dict in dictList) await InvalidateAsync(dict.Name);
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Dictionaries.List)]
    public Task<PagedResultDto<DictVo>> GetListAsync(DictQueryCriteria dictQueryCriteria, CancellationToken ct = default)
        => PageAsync<DictVo>(dictQueryCriteria,
            options => options.IncludePropertyNameList = [nameof(Dict.DictDetails)], ct);

    /// <inheritdoc />
    [UseCache(Expiration = 30, KeyPrefix = GlobalConstants.CachePrefix.LoadDictByName)]
    [Authorize]
    public async Task<DictVo?> QueryByNameAsync(string name, CancellationToken ct = default)
    {
        // Includes 的表达式由 ORM 翻译，导航集合在加载前允许为空。
        var dict = await Table.Where(x => x.Name == name).Includes(x => x.DictDetails!.OrderBy(y => y.DictSort).ToList())
            .FirstAsync(ct);
        return dict == null ? null : Mapper.MapTo<DictVo>(dict);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Dictionaries.Download)]
    public async Task<List<ExportBase>> DownloadAsync(DictQueryCriteria dictQueryCriteria, CancellationToken ct = default)
    {
        var conditionalModels = dictQueryCriteria.ApplyQueryConditionalModel();
        var dicts = await Table.Includes(x => x.DictDetails).Where(conditionalModels)
            .ToListAsync(ct);
        List<ExportBase> dictExports = new List<ExportBase>();

        dicts.ForEach(x =>
        {
            dictExports.AddRange((x.DictDetails ?? []).Select(d => new DictExport
            {
                Id = x.Id,
                DictType = x.DictType,
                Name = x.Name,
                Description = x.Description!,
                Lable = d.Label,
                Value = d.Value,
                CreationTime = x.CreationTime
            }));
        });

        return dictExports;
    }

    private Task InvalidateAsync(string name) => _unitOfWork.AfterCommitAsync(() =>
        _cache.InvalidateAsync(GlobalConstants.CachePrefix.LoadDictByName + name.ToMd5String16()));

    #endregion

}
