using Ape.Volo.Common.IdGenerator;
using System.Globalization;

namespace Ape.Volo.Web.Storage;

/// <summary>
/// Web 根目录下的本地上传存储，统一负责命名、路径约束和失败清理。
/// </summary>
/// <param name="environment">宿主环境，提供 Web 根目录</param>
/// <param name="clock">时间提供者</param>
/// <param name="logger">日志记录器</param>
public sealed class LocalFileStorage(IWebHostEnvironment environment, TimeProvider clock, ILogger<LocalFileStorage> logger) : IFileStorage
{
    private string Root => Path.GetFullPath(environment.WebRootPath
        ?? throw new InvalidOperationException("宿主未配置 Web 根目录。"));

    /// <inheritdoc />
    public async Task<StoredFile> SaveAsync(UploadFile file, string category, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(category) || category.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not '-' and not '_'))
            throw new ArgumentException("存储类别不能包含路径分隔符。", nameof(category));
        var name = clock.GetLocalNow().ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) + "_" + IdHelper.NextId() + Path.GetExtension(file.FileName);
        var relative = $"/uploads/file/{category}/{name}";
        var fullPath = Resolve(relative);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var created = false;
        try
        {
            await using (var destination = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                created = true;
                await file.CopyToAsync(destination, ct);
                await destination.FlushAsync(ct);
            }
            return new StoredFile(name, relative);
        }
        catch
        {
            if (created) Delete(relative);
            throw;
        }
    }

    /// <inheritdoc />
    public bool Delete(string path)
    {
        var fullPath = Resolve(path);
        try { File.Delete(fullPath); return true; }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            logger.LogError(exception, "清理上传文件失败。");
            return false;
        }
    }

    private string Resolve(string path)
    {
        if (!path.StartsWith("/uploads/file/", StringComparison.Ordinal) || path.Contains('\\') || path.Contains(':'))
            throw new ArgumentException("只能访问上传存储内的相对路径。", nameof(path));
        var root = Path.Combine(Root, "uploads", "file") + Path.DirectorySeparatorChar;
        var resolved = Path.GetFullPath(Path.Combine(Root, path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!resolved.StartsWith(root, comparison)) throw new ArgumentException("路径不能越过上传存储目录。", nameof(path));
        return resolved;
    }
}
