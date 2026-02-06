using System.Collections.Generic;
using System.Threading.Tasks;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Extensions;
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

namespace Ape.Volo.Business.System;

/// <summary>
/// 字典详情服务
/// </summary>
public class DictDetailService : BaseServices<DictDetail>, IDictDetailService
{
    #region 基础方法

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateDictDetailDto"></param>
    /// <returns></returns>
    public async Task<OperateResult> CreateAsync(CreateUpdateDictDetailDto createUpdateDictDetailDto)
    {
        if (
            await TableWhere(x =>
                x.DictId == createUpdateDictDetailDto.DictId && x.Label == createUpdateDictDetailDto.Label).AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateDictDetailDto,
                nameof(createUpdateDictDetailDto.Label)));
        }

        if (await TableWhere(x =>
                x.DictId == createUpdateDictDetailDto.DictId && x.Value == createUpdateDictDetailDto.Value).AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateDictDetailDto,
                nameof(createUpdateDictDetailDto.Value)));
        }

        var dictDetail = App.Mapper.MapTo<DictDetail>(createUpdateDictDetailDto);
        var result = await AddAsync(dictDetail);
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateDictDetailDto"></param>
    /// <returns></returns>
    public async Task<OperateResult> UpdateAsync(CreateUpdateDictDetailDto createUpdateDictDetailDto)
    {
        var oldDictDetail =
            await TableWhere(x => x.Id == createUpdateDictDetailDto.Id).FirstAsync();

        if (oldDictDetail.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist(createUpdateDictDetailDto,
                LanguageKeyConstants.DictDetail,
                nameof(createUpdateDictDetailDto.Id)));
        }

        if (oldDictDetail.Label != createUpdateDictDetailDto.Label &&
            await TableWhere(x =>
                x.DictId == createUpdateDictDetailDto.DictId && x.Label == createUpdateDictDetailDto.Label).AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateDictDetailDto,
                nameof(createUpdateDictDetailDto.Label)));
        }

        if (oldDictDetail.Value != createUpdateDictDetailDto.Value &&
            await TableWhere(x =>
                x.DictId == createUpdateDictDetailDto.DictId && x.Value == createUpdateDictDetailDto.Value).AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateDictDetailDto,
                nameof(createUpdateDictDetailDto.Value)));
        }


        var dictDetail = App.Mapper.MapTo<DictDetail>(createUpdateDictDetailDto);
        var result = await UpdateAsync(dictDetail);
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

        var result = await LogicDelete<DictDetail>(x => ids.Contains(x.Id));
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 获取字典详情
    /// </summary>
    /// <param name="dictId">字典ID</param>
    /// <returns></returns>
    [UseCache(Expiration = 30, KeyPrefix = GlobalConstants.CachePrefix.LoadDictDetailByDictId)]
    public async Task<List<DictDetailVo>> GetDetailByDictIdAsync(long dictId)
    {
        var dictDetail = await TableWhere(x => x.DictId == dictId).OrderBy(x => x.DictSort).ToListAsync();

        return App.Mapper.MapTo<List<DictDetailVo>>(dictDetail);
    }

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="dictDetailQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    public async Task<List<DictDetailVo>> QueryAsync(DictDetailQueryCriteria dictDetailQueryCriteria,
        Pagination pagination)
    {
        // if (dictDetailQueryCriteria.DictId > 0)
        // {
        //     dictDetailQueryCriteria.DictName = "";
        // }
        //
        // if (!dictDetailQueryCriteria.DictName.IsNullOrEmpty())
        // {
        //     var dict = await SugarClient.Queryable<Dict>().Where(x => x.Name == dictDetailQueryCriteria.DictName)
        //         .FirstAsync();
        //     if (dict == null)
        //     {
        //         return new List<DictDetailVo>();
        //     }
        //
        //     var dictDetailList = await App.GetService<IDictDetailService>().GetDetailByDictIdAsync(dict.Id);
        //     return dictDetailList;
        // }

        // pagination.SortField = "dictSort";
        // pagination.OrderByType = OrderByType.Asc;
        var queryOptions = new QueryOptions<DictDetail>
        {
            Pagination = pagination,
            ConditionalModels = dictDetailQueryCriteria.ApplyQueryConditionalModel()
        };
        var list = await TablePageAsync(queryOptions);
        return App.Mapper.MapTo<List<DictDetailVo>>(list);
    }

    #endregion
}
