using Ape.Volo.Application.Contracts.Abstractions.Storage;
using Ape.Volo.Application.Contracts.SystemManagement.FileRecords.Models;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.SystemManagement.FileRecords;

/// <summary>
/// 文件记录接口
/// </summary>
public interface IFileRecordService : IApplicationService
{
    #region 基础接口

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="file"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    [RemoteService(false)]
    Task<OperateResult> CreateAsync(UploadFile file, CancellationToken ct = default);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateFileRecordDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> UpdateAsync(CreateUpdateFileRecordDto createUpdateFileRecordDto, CancellationToken ct = default);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> DeleteAsync(HashSet<long> ids, CancellationToken ct = default);

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="fileRecordQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<PagedResultDto<FileRecordVo>> GetListAsync(FileRecordQueryCriteria fileRecordQueryCriteria, CancellationToken ct = default);

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="fileRecordQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<ExportBase>> DownloadAsync(FileRecordQueryCriteria fileRecordQueryCriteria, CancellationToken ct = default);

    #endregion
}
