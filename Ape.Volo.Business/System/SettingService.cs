using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Model;
using Ape.Volo.Core;
using Ape.Volo.Core.Utils;
using Ape.Volo.Entity.Core.System;
using Ape.Volo.IBusiness.System;
using Ape.Volo.SharedModel.Dto.Core.System;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.System;
using Ape.Volo.ViewModel.Core.System;
using Ape.Volo.ViewModel.Report.System;
using Microsoft.Extensions.Logging;
using static Ape.Volo.Common.Helper.ExceptionHelper;

namespace Ape.Volo.Business.System;

/// <summary>
/// 全局设置服务
/// </summary>
public class SettingService : BaseServices<Setting>, ISettingService
{
    #region 基础方法

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateSettingDto"></param>
    /// <returns></returns>
    public async Task<OperateResult> CreateAsync(CreateUpdateSettingDto createUpdateSettingDto)
    {
        if (await TableWhere(r => r.Name == createUpdateSettingDto.Name).AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateSettingDto,
                nameof(createUpdateSettingDto.Name)));
        }

        var setting = App.Mapper.MapTo<Setting>(createUpdateSettingDto);
        var result = await AddAsync(setting);
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateSettingDto"></param>
    /// <returns></returns>
    public async Task<OperateResult> UpdateAsync(CreateUpdateSettingDto createUpdateSettingDto)
    {
        //取出待更新数据
        var oldSetting = await TableWhere(x => x.Id == createUpdateSettingDto.Id).FirstAsync();
        if (oldSetting.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist(createUpdateSettingDto,
                LanguageKeyConstants.Parameter,
                nameof(createUpdateSettingDto.Id)));
        }

        if (oldSetting.Name != createUpdateSettingDto.Name &&
            await TableWhere(x => x.Name == createUpdateSettingDto.Name).AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateSettingDto,
                nameof(createUpdateSettingDto.Name)));
        }

        await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.LoadSettingByName +
                                    oldSetting.Name.ToMd5String16());
        var setting = App.Mapper.MapTo<Setting>(createUpdateSettingDto);
        var result = await UpdateAsync(setting);
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public async Task<OperateResult> DeleteAsync(HashSet<long> ids)
    {
        var settings = await TableWhere(x => ids.Contains(x.Id)).ToListAsync();
        if (settings.Count == 0)
        {
            return OperateResult.Error(ValidationError.NotExist());
        }

        foreach (var setting in settings)
        {
            await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.LoadSettingByName +
                                        setting.Name.ToMd5String16());
        }

        var result = await LogicDelete<Setting>(x => ids.Contains(x.Id));

        return OperateResult.Result(result);
    }

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="parameterQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    public async Task<List<SettingVo>> QueryAsync(ParameterQueryCriteria parameterQueryCriteria,
        Pagination pagination)
    {
        var queryOptions = new QueryOptions<Setting>
        {
            Pagination = pagination, ConditionalModels = parameterQueryCriteria.ApplyQueryConditionalModel()
        };
        return App.Mapper.MapTo<List<SettingVo>>(await TablePageAsync(queryOptions));
    }

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="parameterQueryCriteria"></param>
    /// <returns></returns>
    public async Task<List<ExportBase>> DownloadAsync(ParameterQueryCriteria parameterQueryCriteria)
    {
        var settings = await TableWhere(parameterQueryCriteria.ApplyQueryConditionalModel()).ToListAsync();
        List<ExportBase> settingExports = new List<ExportBase>();
        settingExports.AddRange(settings.Select(x => new SettingExport
        {
            Id = x.Id,
            Name = x.Name,
            Value = x.Value,
            Enabled = x.Enabled,
            Description = x.Description,
            CreateTime = x.CreateTime
        }));
        return settingExports;
    }

    /// <summary>
    /// 获取设置 值
    /// </summary>
    /// <param name="settingName"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [UseCache(Expiration = 30, KeyPrefix = GlobalConstants.CachePrefix.LoadSettingByName)]
    public async Task<T> GetSettingValue<T>(string settingName)
    {
        var setting = await TableWhere(x => x.Name == settingName).FirstAsync();

        if (setting == null) return default;

        try
        {
            return (T)ConvertValue(typeof(T), setting.Value);
        }
        catch (Exception e)
        {
            App.GetService<ILogger<Setting>>().LogError(GetExceptionAllMsg(e));
            return default;
        }
    }

    /// <summary>
    /// 类型转换
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    private static object ConvertValue(Type type, string value)
    {
        if (type == typeof(object))
        {
            return value;
        }

        if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return string.IsNullOrEmpty(value) ? value : ConvertValue(Nullable.GetUnderlyingType(type), value);
        }

        var converter = TypeDescriptor.GetConverter(type);
        return converter.CanConvertFrom(typeof(string)) ? converter.ConvertFromInvariantString(value) : null;
    }

    #endregion
}