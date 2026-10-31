using Ape.Volo.Application.Contracts.Authorization;
using System.Linq.Expressions;
using Ape.Volo.Application.Contracts.Organization.Models;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Shared.Model;
using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Core.Utils;
using Ape.Volo.Entity.Organization;
using SqlSugar;

namespace Ape.Volo.Application.Organization;

/// <summary>
/// 岗位服务
/// </summary>
public class PositionService : BaseService<Position>, IPositionService
{
    #region 构造函数

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sugarRepository">当前实体的仓储</param>
    /// <param name="mapper">对象映射器</param>
    public PositionService(ISugarRepository<Position> sugarRepository, IMapper mapper) : base(sugarRepository, mapper)
    {
    }

    #endregion

    #region 基础方法

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Positions.Add)]
    public async Task<OperateResult> CreateAsync(CreateUpdatePositionDto createUpdatePositionDto, CancellationToken ct = default)
    {
        if (await TableWhere(j => j.Name == createUpdatePositionDto.Name).AnyAsync(ct))
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdatePositionDto,
                nameof(createUpdatePositionDto.Name)));
        }

        var position = Mapper.MapTo<Position>(createUpdatePositionDto);
        var result = await AddAsync(position, ct: ct);
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Positions.Edit)]
    public async Task<OperateResult> UpdateAsync(CreateUpdatePositionDto createUpdatePositionDto, CancellationToken ct = default)
    {
        var oldJob =
            await TableWhere(x => x.Id == createUpdatePositionDto.Id).FirstAsync(ct);
        if (oldJob.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist(createUpdatePositionDto, "岗位",
                nameof(createUpdatePositionDto.Id)));
        }

        if (oldJob.Name != createUpdatePositionDto.Name &&
            await TableWhere(j => j.Name == createUpdatePositionDto.Name).AnyAsync(ct))
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdatePositionDto,
                nameof(createUpdatePositionDto.Name)));
        }

        var position = Mapper.MapTo<Position>(createUpdatePositionDto);
        var result = await UpdateAsync(position, ct: ct);
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Positions.Delete)]
    public async Task<OperateResult> DeleteAsync(HashSet<long> ids, CancellationToken ct = default)
    {
        var positionIds = await TableWhere(x => ids.Contains(x.Id)).Select(x => x.Id).ToListAsync(ct);
        if (positionIds.Count < 1)
        {
            return OperateResult.Error(ValidationError.NotExist());
        }

        if (await SugarClient.Queryable<UserPosition>()
                .AnyAsync(link => positionIds.Contains(link.PositionId), ct))
        {
            return OperateResult.Error(ValidationError.DataAssociationExists());
        }

        var result = await LogicDeleteAsync(x => ids.Contains(x.Id), ct);
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Positions.List)]
    public Task<PagedResultDto<PositionVo>> GetListAsync(PositionQueryCriteria positionQueryCriteria, CancellationToken ct = default)
        => PageAsync<PositionVo>(positionQueryCriteria, ct: ct);

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Positions.Download)]
    public async Task<List<ExportBase>> DownloadAsync(PositionQueryCriteria positionQueryCriteria, CancellationToken ct = default)
    {
        var conditionalModels = positionQueryCriteria.ApplyQueryConditionalModel();
        var positions = await TableWhere(conditionalModels).ToListAsync(ct);
        List<ExportBase> roleExports = new List<ExportBase>();
        roleExports.AddRange(Mapper.MapTo<List<PositionExport>>(positions));
        return roleExports;
    }

    #endregion

    #region 扩展方法

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Positions.List, SystemPermissions.Users.List, SystemPermissions.Users.Add, SystemPermissions.Users.Edit)]
    public async Task<List<PositionVo>> GetAllAsync(CancellationToken ct = default)
    {
        Expression<Func<Position, bool>> whereExpression = x => x.Enabled;


        return Mapper.MapTo<List<PositionVo>>(await TableWhere(whereExpression, null, x => x.Sort, OrderByType.Asc)
            .ToListAsync(ct));
    }

    #endregion

}
