using Ape.Volo.Application.Contracts.Authorization;
using Ape.Volo.Application.Contracts.Abstractions.Storage;
using Ape.Volo.Application.Contracts.SystemManagement.FileRecords.Models;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Helper;
using Ape.Volo.Shared.Model;
using Ape.Volo.Core.ConfigOptions;
using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Core.Utils;
using Ape.Volo.Entity.SystemManagement;

namespace Ape.Volo.Application.SystemManagement.FileRecords;

/// <summary>
/// 文件记录服务
/// </summary>
public class FileRecordService : BaseService<FileRecord>, IFileRecordService
{
    #region 构造函数

    private readonly IFileStorage _fileStorage;

    private readonly IOptions<SystemOptions> _systemOptions;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sugarRepository">当前实体的仓储</param>
    /// <param name="fileStorage">上传文件存储路径</param>
    /// <param name="mapper">对象映射器</param>
    /// <param name="systemOptions">系统配置</param>
    public FileRecordService(ISugarRepository<FileRecord> sugarRepository, IFileStorage fileStorage,
        IMapper mapper, IOptions<SystemOptions> systemOptions)
        : base(sugarRepository, mapper)
    {
        _fileStorage = fileStorage;
        _systemOptions = systemOptions;
    }

    #endregion

    #region 基础方法

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Files.Add)]
    public async Task<OperateResult> CreateAsync(UploadFile file, CancellationToken ct = default)
    {
        if (file.Length <= 0)
        {
            return OperateResult.Error("不能上传空文件");
        }

        var fileLimitSizeMb = _systemOptions.Value.FileLimitSize;
        if (file.Length > fileLimitSizeMb * 1024L * 1024L)
        {
            return OperateResult.Error($"文件过大，请选择小于等于 {fileLimitSizeMb}MB 的文件");
        }

        if (!FileUploadPolicy.IsAllowedGeneralFileName(file.FileName))
        {
            return OperateResult.Error("不支持该文件类型");
        }

        var fileExtensionName = FileHelper.GetExtensionName(file.FileName);
        var fileTypeName = FileHelper.GetFileTypeName(fileExtensionName);
        var fileTypeNameEn = FileHelper.GetFileTypeNameEn(fileTypeName);

        var stored = await _fileStorage.SaveAsync(file, fileTypeNameEn, ct);
        var fileRecord = new FileRecord
        {
            Description = file.FileName,
            OriginalName = file.FileName,
            NewName = stored.Name,
            FilePath = stored.Path,
            Size = FileHelper.GetFileSize(file.Length),
            ContentType = file.ContentType,
            ContentTypeName = fileTypeName,
            ContentTypeNameEn = fileTypeNameEn
        };
        var saved = false;
        try
        {
            saved = await AddAsync(fileRecord, ct: ct);
            return OperateResult.Result(saved);
        }
        finally
        {
            if (!saved) _fileStorage.Delete(stored.Path);
        }
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Files.Edit)]
    public async Task<OperateResult> UpdateAsync(CreateUpdateFileRecordDto createUpdateFileRecordDto, CancellationToken ct = default)
    {
        //取出待更新数据
        var oldFileRecord = await TableWhere(x => x.Id == createUpdateFileRecordDto.Id).FirstAsync(ct);
        if (oldFileRecord.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist(createUpdateFileRecordDto,
                "文件记录",
                nameof(createUpdateFileRecordDto.Id)));
        }

        var fileRecord = Mapper.MapTo<FileRecord>(createUpdateFileRecordDto);
        var result = await UpdateAsync(fileRecord, ct: ct);
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Files.Delete)]
    public async Task<OperateResult> DeleteAsync(HashSet<long> ids, CancellationToken ct = default)
    {
        var files = await TableWhere(x => ids.Contains(x.Id)).ToListAsync(ct);
        if (files.Count == 0)
        {
            return OperateResult.Error(ValidationError.NotExist());
        }

        if (!await LogicDeleteAsync(x => ids.Contains(x.Id), ct)) return OperateResult.Error("删除文件记录失败。");
        var cleaned = true;
        foreach (var file in files)
        {
            if (!string.IsNullOrEmpty(file.FilePath)) cleaned = _fileStorage.Delete(file.FilePath) && cleaned;
        }

        return cleaned ? OperateResult.Success() : OperateResult.Error("记录已删除，但部分文件清理失败。");
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Files.List)]
    public Task<PagedResultDto<FileRecordVo>> GetListAsync(FileRecordQueryCriteria fileRecordQueryCriteria, CancellationToken ct = default)
        => PageAsync<FileRecordVo>(fileRecordQueryCriteria, ct: ct);

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Files.Download)]
    public async Task<List<ExportBase>> DownloadAsync(FileRecordQueryCriteria fileRecordQueryCriteria, CancellationToken ct = default)
    {
        var conditionalModels = fileRecordQueryCriteria.ApplyQueryConditionalModel();
        var fileRecords = await TableWhere(conditionalModels).ToListAsync(ct);
        List<ExportBase> fileRecordExports = new List<ExportBase>();
        fileRecordExports.AddRange(Mapper.MapTo<List<FileRecordExport>>(fileRecords));
        return fileRecordExports;
    }

    #endregion

}
