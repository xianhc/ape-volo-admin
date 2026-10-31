namespace Ape.Volo.Application.Contracts.Abstractions.Storage;

/// <summary>
/// 上传文件存储端口，应用只接触公开相对路径，不接触宿主物理目录。
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// 保存到指定类别，失败时清理未写完的新文件。
    /// </summary>
    /// <param name="file">待保存的上传文件</param>
    /// <param name="category">存储类别，只允许字母、数字、连字符和下划线</param>
    /// <param name="ct">取消令牌</param>
    Task<StoredFile> SaveAsync(UploadFile file, string category, CancellationToken ct = default);
    /// <summary>
    /// 删除存储对象，不存在视为成功；I/O 失败返回 false，非法路径拒绝执行。
    /// </summary>
    /// <param name="path">上传存储内的公开相对路径</param>
    bool Delete(string path);
}

/// <summary>
/// 已保存文件的公开元数据。
/// </summary>
/// <param name="Name">存储文件名。</param>
/// <param name="Path">公开相对路径。</param>
public sealed record StoredFile(string Name, string Path);
