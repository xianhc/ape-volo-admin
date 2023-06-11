using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ApeVolo.Business.Base;
using ApeVolo.Common.AttributeExt;
using ApeVolo.Common.Caches.Redis.Service;
using ApeVolo.Common.Exception;
using ApeVolo.Common.Extention;
using ApeVolo.Common.Global;
using ApeVolo.Common.Model;
using ApeVolo.Common.Resources;
using ApeVolo.Common.WebApp;
using ApeVolo.IBusiness.Dto.System.Setting;
using ApeVolo.IBusiness.ExportModel.System;
using ApeVolo.IBusiness.Interface.System.Setting;
using ApeVolo.IBusiness.QueryModel;
using ApeVolo.IRepository.System.Setting;
using AutoMapper;

namespace ApeVolo.Business.System.Setting;

public class SettingService : BaseServices<Entity.System.Setting>, ISettingService
{
    #region 字段

    private readonly IRedisCacheService _redisCacheService;

    #endregion

    #region 构造函数

    public SettingService(IMapper mapper, ISettingRepository settingRepository, ICurrentUser currentUser,
        IRedisCacheService redisCacheService)
    {
        Mapper = mapper;
        BaseDal = settingRepository;
        CurrentUser = currentUser;
        _redisCacheService = redisCacheService;
    }

    #endregion

    #region 基础方法

    public async Task<bool> CreateAsync(CreateUpdateSettingDto createUpdateSettingDto)
    {
        if (await IsExistAsync(r => r.Name == createUpdateSettingDto.Name))
        {
            throw new BadRequestException(Localized.Get("{0}{1}IsExist", Localized.Get("Setting"),
                createUpdateSettingDto.Name));
        }

        var setting = Mapper.Map<Entity.System.Setting>(createUpdateSettingDto);
        return await AddEntityAsync(setting);
    }

    public async Task<bool> UpdateAsync(CreateUpdateSettingDto createUpdateSettingDto)
    {
        //取出待更新数据
        var oldSetting = await QueryFirstAsync(x => x.Id == createUpdateSettingDto.Id);
        if (oldSetting.IsNull())
        {
            throw new BadRequestException(Localized.Get("DataNotExist"));
        }

        if (oldSetting.Name != createUpdateSettingDto.Name &&
            await IsExistAsync(x => x.Name == createUpdateSettingDto.Name))
        {
            throw new BadRequestException(Localized.Get("{0}{1}IsExist", Localized.Get("Setting"),
                createUpdateSettingDto.Name));
        }

        await _redisCacheService.RemoveAsync(RedisKey.LoadSettingByName + oldSetting.Name.ToMd5String16());
        var setting = Mapper.Map<Entity.System.Setting>(createUpdateSettingDto);
        return await UpdateEntityAsync(setting);
    }

    public async Task<bool> DeleteAsync(HashSet<long> ids)
    {
        var settings = await QueryByIdsAsync(ids);
        foreach (var setting in settings)
        {
            await _redisCacheService.RemoveAsync(RedisKey.LoadSettingByName + setting.Name.ToMd5String16());
        }

        return await DeleteEntityListAsync(settings);
    }

    public async Task<List<SettingDto>> QueryAsync(SettingQueryCriteria settingQueryCriteria, Pagination pagination)
    {
        Expression<Func<Entity.System.Setting, bool>> whereLambda = r => true;
        if (!settingQueryCriteria.KeyWords.IsNullOrEmpty())
        {
            whereLambda = whereLambda.AndAlso(r =>
                r.Name.Contains(settingQueryCriteria.KeyWords) || r.Value.Contains(settingQueryCriteria.KeyWords) ||
                r.Description.Contains(settingQueryCriteria.KeyWords));
        }

        if (!settingQueryCriteria.Enabled.IsNullOrEmpty())
        {
            whereLambda = whereLambda.AndAlso(x => x.Enabled == settingQueryCriteria.Enabled);
        }

        if (!settingQueryCriteria.CreateTime.IsNull())
        {
            whereLambda = whereLambda.AndAlso(r =>
                r.CreateTime >= settingQueryCriteria.CreateTime[0] &&
                r.CreateTime <= settingQueryCriteria.CreateTime[1]);
        }

        return Mapper.Map<List<SettingDto>>(await BaseDal.QueryPageListAsync(whereLambda, pagination));
    }

    public async Task<List<ExportBase>> DownloadAsync(SettingQueryCriteria settingQueryCriteria)
    {
        var settings = await QueryAsync(settingQueryCriteria, new Pagination { PageSize = 9999 });
        List<ExportBase> settingExports = new List<ExportBase>();
        settingExports.AddRange(settings.Select(x => new SettingExport()
        {
            Name = x.Name,
            Value = x.Value,
            EnabledState = x.Enabled ? EnabledState.Enabled : EnabledState.Disabled,
            Description = x.Description,
            CreateTime = x.CreateTime
        }));
        return settingExports;
    }

    [RedisCaching(Expiration = 20, KeyPrefix = RedisKey.LoadSettingByName)]
    public async Task<SettingDto> FindSettingByName(string settingName)
    {
        if (settingName.IsNullOrEmpty())
        {
            throw new BadRequestException(Localized.Get("{0}required", "settingName"));
        }

        var setting =
            Mapper.Map<SettingDto>(await QueryFirstAsync(x => x.Name == settingName));
        if (setting.IsNull())
        {
            throw new BadRequestException(Localized.Get("DataNotExist"));
        }

        return setting;
    }

    #endregion
}