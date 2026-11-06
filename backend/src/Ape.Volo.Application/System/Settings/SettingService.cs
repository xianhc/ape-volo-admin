using System.ComponentModel;
using System.Reflection;
using Ape.Volo.Application.Contracts.Authorization;
using Ape.Volo.Application.Contracts.System.Settings;
using Ape.Volo.Application.Contracts.System.Settings.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Core.Repository.UnitOfWork;
using Ape.Volo.Entity.System;
using Ape.Volo.Shared.Model;
using Microsoft.Extensions.Logging;
using static Ape.Volo.Common.Helper.ExceptionHelper;
namespace Ape.Volo.Application.System.Settings;

/// <summary>
/// 全局设置服务
/// </summary>
public class SettingService : BaseService<Setting>, ISettingService
{
    #region 字段

    private readonly ICache _cache;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISugarQueryProvider _sugarQueries;

    private readonly ILogger<Setting> _logger;

    #endregion

    #region 构造函数

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sugarRepository">当前实体的仓储</param>
    /// <param name="mapper">对象映射器</param>
    /// <param name="cache">缓存</param>
    /// <param name="logger">日志</param>
    /// <param name="unitOfWork">提交后失效登记。</param>
    /// <param name="sugarQueries">全局运行参数读取。</param>
    public SettingService(ISugarRepository<Setting> sugarRepository, IMapper mapper, ICache cache,
        ILogger<Setting> logger, IUnitOfWork unitOfWork, ISugarQueryProvider sugarQueries)
        : base(sugarRepository, mapper)
    {
        _cache = cache;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _sugarQueries = sugarQueries;
    }

    #endregion

    #region 基础方法

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Settings.Add)]
    [UseTran]
    public async Task<OperateResult> CreateAsync(CreateUpdateSettingDto createUpdateSettingDto, CancellationToken ct = default)
    {
        if (await TableWhere(r => r.Name == createUpdateSettingDto.Name).AnyAsync(ct))
        {
            return AlreadyExists(() => createUpdateSettingDto.Name);
        }

        var setting = Mapper.MapTo<Setting>(createUpdateSettingDto);
        var result = await AddAsync(setting, ct: ct);
        if (result) await InvalidateAsync(setting.Name);
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Settings.Edit)]
    [UseTran]
    public async Task<OperateResult> UpdateAsync(CreateUpdateSettingDto createUpdateSettingDto, CancellationToken ct = default)
    {
        //取出待更新数据
        var oldSetting = await TableWhere(x => x.Id == createUpdateSettingDto.Id).FirstAsync(ct);
        if (oldSetting.IsNull())
        {
            return NotFound(createUpdateSettingDto.Id);
        }

        if (oldSetting.Name != createUpdateSettingDto.Name &&
            await TableWhere(x => x.Name == createUpdateSettingDto.Name).AnyAsync(ct))
        {
            return AlreadyExists(() => createUpdateSettingDto.Name);
        }

        var setting = Mapper.MapTo<Setting>(createUpdateSettingDto);
        var result = await UpdateAsync(setting, ct: ct);
        if (result)
        {
            await InvalidateAsync(oldSetting.Name);
            await InvalidateAsync(setting.Name);
        }
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Settings.Delete)]
    [UseTran]
    public async Task<OperateResult> DeleteAsync(HashSet<long> ids, CancellationToken ct = default)
    {
        var settings = await TableWhere(x => ids.Contains(x.Id)).ToListAsync(ct);
        if (settings.Count == 0)
        {
            return NotFound();
        }

        var result = await LogicDeleteAsync(x => ids.Contains(x.Id), ct);
        if (result)
            foreach (var setting in settings) await InvalidateAsync(setting.Name);

        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Settings.List)]
    public Task<PagedResultDto<SettingVo>> GetListAsync(ParameterQueryCriteria parameterQueryCriteria, CancellationToken ct = default)
        => PageAsync<SettingVo>(parameterQueryCriteria, ct: ct);

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Settings.Download)]
    public async Task<List<ExportBase>> DownloadAsync(ParameterQueryCriteria parameterQueryCriteria, CancellationToken ct = default)
    {
        var settings = await TableWhere(parameterQueryCriteria.ApplyQueryConditionalModel()).ToListAsync(ct);
        List<ExportBase> settingExports = new List<ExportBase>();
        settingExports.AddRange(Mapper.MapTo<List<SettingExport>>(settings));
        return settingExports;
    }

    /// <inheritdoc />
    [UseCache(Expiration = 30, KeyPrefix = GlobalConstants.CachePrefix.LoadSettingByName)]
    public async Task<T?> GetSettingValue<T>(string settingName, CancellationToken ct = default)
    {
        var setting = await _sugarQueries.QueryWithoutDataScopeAsync(db => db.Queryable<Setting>()
            .Where(x => x.Name == settingName).FirstAsync(ct), ct);

        if (setting == null) return default;

        try
        {
            return (T?)ConvertValue(typeof(T), setting.Value);
        }
        catch (Exception e)
        {
            _logger.LogError(GetExceptionAllMsg(e));
            return default;
        }
    }

    private Task InvalidateAsync(string name) => _unitOfWork.AfterCommitAsync(() =>
        _cache.InvalidateAsync(GlobalConstants.CachePrefix.LoadSettingByName + name));

    /// <summary>
    /// 类型转换
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    private static object? ConvertValue(Type type, string value)
    {
        if (type == typeof(object))
        {
            return value;
        }

        if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return string.IsNullOrEmpty(value) ? value : ConvertValue(Nullable.GetUnderlyingType(type)!, value);
        }

        var converter = TypeDescriptor.GetConverter(type);
        return converter.CanConvertFrom(typeof(string)) ? converter.ConvertFromInvariantString(value) : null;
    }

    #endregion

}
