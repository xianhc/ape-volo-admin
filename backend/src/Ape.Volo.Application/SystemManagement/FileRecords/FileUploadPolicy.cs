namespace Ape.Volo.Application.SystemManagement.FileRecords;

/// <summary>
/// 约束会写入静态目录的上传文件类型。
/// </summary>
public static class FileUploadPolicy
{
    private static readonly HashSet<string> BlockedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".html", ".htm", ".xhtml", ".svg", ".js", ".mjs", ".cjs", ".css", ".xml", ".xsl", ".xslt",
        ".wasm", ".exe", ".dll", ".com", ".bat", ".cmd", ".ps1", ".sh", ".php", ".aspx", ".ashx",
        ".cshtml", ".config"
    };

    private static readonly HashSet<string> AvatarExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"
    };

    /// <summary>
    /// 判断通用上传文件的扩展名是否允许，命中黑名单一律拒绝
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <returns>允许上传返回 true</returns>
    public static bool IsAllowedGeneralFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        return !string.IsNullOrWhiteSpace(extension) && !BlockedExtensions.Contains(extension);
    }

    /// <summary>
    /// 判断头像文件扩展名是否在白名单内
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <returns>允许上传返回 true</returns>
    public static bool IsAllowedAvatarFileName(string fileName)
    {
        return AvatarExtensions.Contains(Path.GetExtension(fileName));
    }
}
