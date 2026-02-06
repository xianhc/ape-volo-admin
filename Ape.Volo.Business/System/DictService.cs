using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Model;
using Ape.Volo.Core;
using Ape.Volo.Core.Utils;
using Ape.Volo.Entity.Core.System.Dict;
using Ape.Volo.IBusiness.System;
using Ape.Volo.SharedModel.Dto.Core.System.Dict;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.System;
using Ape.Volo.ViewModel.Core.System.Dict;
using Ape.Volo.ViewModel.Report.System;
using ConditionalModelExtensions = Ape.Volo.Common.Extensions.ConditionalModelExtensions;
using ExtObject = Ape.Volo.Common.Extensions.ExtObject;

namespace Ape.Volo.Business.System;

/// <summary>
/// 字典服务
/// </summary>
public class DictService : BaseServices<Dict>, IDictService
{
    #region 基础方法

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateDictDto"></param>
    /// <returns></returns>
    public async Task<OperateResult> CreateAsync(CreateUpdateDictDto createUpdateDictDto)
    {
        if (await TableWhere(d => d.Name == createUpdateDictDto.Name).AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateDictDto, nameof(createUpdateDictDto.Name)));
        }

        var result = await AddAsync(App.Mapper.MapTo<Dict>(createUpdateDictDto));
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateDictDto"></param>
    /// <returns></returns>
    public async Task<OperateResult> UpdateAsync(CreateUpdateDictDto createUpdateDictDto)
    {
        var oldDict =
            await TableWhere(x => x.Id == createUpdateDictDto.Id).FirstAsync();
        if (ExtObject.IsNull(oldDict))
        {
            return OperateResult.Error(ValidationError.NotExist(createUpdateDictDto, LanguageKeyConstants.Dict,
                nameof(createUpdateDictDto.Id)));
        }

        if (oldDict.Name != createUpdateDictDto.Name &&
            await TableWhere(j => j.Name == createUpdateDictDto.Name).AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateDictDto, nameof(createUpdateDictDto.Name)));
        }

        var result = await UpdateAsync(App.Mapper.MapTo<Dict>(createUpdateDictDto));
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public async Task<OperateResult> DeleteAsync(HashSet<long> ids)
    {
        var dictList = await TableWhere(x => ids.Contains(x.Id)).ToListAsync();
        if (dictList.Count <= 0)
        {
            return OperateResult.Error(ValidationError.NotExist());
        }

        var result = await LogicDelete<Dict>(x => ids.Contains(x.Id));
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="dictQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    public async Task<List<DictVo>> QueryAsync(DictQueryCriteria dictQueryCriteria, Pagination pagination)
    {
        var queryOptions = new QueryOptions<Dict>
        {
            Pagination = pagination,
            ConditionalModels = ConditionalModelExtensions.ApplyQueryConditionalModel(dictQueryCriteria),
            IsIncludes = true
        };
        var list = await TablePageAsync(queryOptions);
        var dicts = App.Mapper.MapTo<List<DictVo>>(list);
        // foreach (var item in dicts)
        // {
        //     item.DictDetails.ForEach(d => d.Dict = new DictDto2 { Id = d.DictId });
        // }

        return dicts;
    }

    /// <summary>
    /// 根据名称查询字典
    /// </summary>
    /// <returns></returns>
    [UseCache(Expiration = 30, KeyPrefix = GlobalConstants.CachePrefix.LoadDictByName)]
    public async Task<DictVo> QueryByNameAsync(string name)
    {
        var dict = await Table.Where(x => x.Name == name).Includes(x => x.DictDetails.OrderBy(y => y.DictSort).ToList())
            .FirstAsync();
        return App.Mapper.MapTo<DictVo>(dict);
    }

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="dictQueryCriteria"></param>
    /// <returns></returns>
    public async Task<List<ExportBase>> DownloadAsync(DictQueryCriteria dictQueryCriteria)
    {
        var conditionalModels = ConditionalModelExtensions.ApplyQueryConditionalModel(dictQueryCriteria);
        var dicts = await Table.Includes(x => x.DictDetails).Where(conditionalModels)
            .ToListAsync();
        List<ExportBase> dictExports = new List<ExportBase>();

        dicts.ForEach(x =>
        {
            dictExports.AddRange(x.DictDetails.Select(d => new DictExport
            {
                Id = x.Id,
                DictType = x.DictType,
                Name = x.Name,
                Description = x.Description,
                Lable = d.Label,
                Value = d.Value,
                CreateTime = x.CreateTime
            }));
        });

        return dictExports;
    }

    #endregion
}
