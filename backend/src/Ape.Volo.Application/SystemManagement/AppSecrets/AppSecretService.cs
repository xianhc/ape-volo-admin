using Ape.Volo.Application.Contracts.Authorization;
using Ape.Volo.Application.Contracts.SystemManagement.AppSecrets.Models;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.IdGenerator;
using Ape.Volo.Shared.Model;
using Ape.Volo.Core.ConfigOptions;
using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Core.Utils;
using Ape.Volo.Entity.SystemManagement;

namespace Ape.Volo.Application.SystemManagement.AppSecrets;

/// <summary>
/// App应用秘钥
/// </summary>
public class AppSecretService : BaseService<AppSecret>, IAppSecretService
{
    #region 字段

    private readonly IOptions<SystemOptions> _systemOptions;

    #endregion

    #region 构造函数

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sugarRepository">当前实体的仓储</param>
    /// <param name="mapper">对象映射器</param>
    /// <param name="systemOptions">系统配置</param>
    public AppSecretService(ISugarRepository<AppSecret> sugarRepository, IMapper mapper,
        IOptions<SystemOptions> systemOptions) : base(sugarRepository, mapper)
    {
        _systemOptions = systemOptions;
    }

    #endregion

    #region 基础方法

    /// <inheritdoc />
    [HasPerm(SystemPermissions.AppSecrets.Add)]
    public async Task<OperateResult> CreateAsync(CreateUpdateAppSecretDto createUpdateAppSecretDto, CancellationToken ct = default)
    {
        if (await TableWhere(r => r.AppName == createUpdateAppSecretDto.AppName).AnyAsync(ct))
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateAppSecretDto,
                nameof(createUpdateAppSecretDto.AppName)));
        }

        var id = IdHelper.NextId().ToString();
        createUpdateAppSecretDto.AppId = DateTime.Now.ToString("yyyyMMdd") + id[..8];
        createUpdateAppSecretDto.AppSecretKey =
            (createUpdateAppSecretDto.AppId + id).ToHmacsha256String(_systemOptions.Value.HmacSecret);
        var appSecret = Mapper.MapTo<AppSecret>(createUpdateAppSecretDto);
        var result = await AddAsync(appSecret, ct: ct);
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.AppSecrets.Edit)]
    public async Task<OperateResult> UpdateAsync(CreateUpdateAppSecretDto createUpdateAppSecretDto, CancellationToken ct = default)
    {
        //取出待更新数据
        var oldAppSecret = await TableWhere(x => x.Id == createUpdateAppSecretDto.Id).FirstAsync(ct);
        if (oldAppSecret.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist(createUpdateAppSecretDto,
                "App应用密钥",
                nameof(createUpdateAppSecretDto.Id)));
        }

        if (oldAppSecret.AppName != createUpdateAppSecretDto.AppName &&
            await TableWhere(x => x.AppName == createUpdateAppSecretDto.AppName).AnyAsync(ct))
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateAppSecretDto,
                nameof(createUpdateAppSecretDto.AppName)));
        }

        var appSecret = Mapper.MapTo<AppSecret>(createUpdateAppSecretDto);
        var result = await UpdateAsync(appSecret, null, x => new
        {
            x.AppId, x.AppSecretKey
        }, ct: ct);
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.AppSecrets.Delete)]
    public async Task<OperateResult> DeleteAsync(HashSet<long> ids, CancellationToken ct = default)
    {
        var appSecrets = await TableWhere(x => ids.Contains(x.Id)).ToListAsync(ct);
        if (appSecrets.Count <= 0)
        {
            return OperateResult.Error(ValidationError.NotExist());
        }

        var result = await LogicDeleteAsync(x => ids.Contains(x.Id), ct);
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.AppSecrets.List)]
    public Task<PagedResultDto<AppSecretVo>> GetListAsync(AppSecretQueryCriteria appsecretQueryCriteria, CancellationToken ct = default)
        => PageAsync<AppSecretVo>(appsecretQueryCriteria, ct: ct);

    /// <inheritdoc />
    [HasPerm(SystemPermissions.AppSecrets.Download)]
    public async Task<List<ExportBase>> DownloadAsync(AppSecretQueryCriteria appsecretQueryCriteria, CancellationToken ct = default)
    {
        var conditionalModels = appsecretQueryCriteria.ApplyQueryConditionalModel();
        var appSecrets = await TableWhere(conditionalModels).ToListAsync(ct);
        List<ExportBase> appSecretExports = new List<ExportBase>();
        appSecretExports.AddRange(Mapper.MapTo<List<AppSecretExport>>(appSecrets));
        return appSecretExports;
    }

    /// <inheritdoc />
    public async Task<AppSecretVo?> QueryByAppIdAsync(string appId, CancellationToken ct = default)
    {
        var appSecret = await TableWhere(x => x.AppId == appId).FirstAsync(ct);
        return appSecret == null ? null : Mapper.MapTo<AppSecretVo>(appSecret);
    }

    #endregion

}
