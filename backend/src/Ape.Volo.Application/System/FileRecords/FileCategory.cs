namespace Ape.Volo.Application.System.FileRecords;

/// <summary>
/// 将标准 MIME 类型归入文件管理的展示类别与存储目录。
/// </summary>
internal sealed record FileCategory(string DisplayName, string DirectoryName)
{
    private static readonly FileCategory Document = new("文档", "documents");
    private static readonly FileCategory Music = new("音乐", "musics");
    private static readonly FileCategory Video = new("视频", "videos");
    private static readonly FileCategory Image = new("图片", "images");
    private static readonly FileCategory Other = new("其他", "other");

    public static FileCategory FromContentType(string contentType)
    {
        if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)) return Image;
        if (contentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase)) return Music;
        if (contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase)) return Video;
        if (contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase) ||
            contentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase) ||
            contentType.Equals("application/rtf", StringComparison.OrdinalIgnoreCase) ||
            contentType.Equals("application/msword", StringComparison.OrdinalIgnoreCase) ||
            contentType.StartsWith("application/vnd.ms-excel", StringComparison.OrdinalIgnoreCase) ||
            contentType.StartsWith("application/vnd.ms-powerpoint", StringComparison.OrdinalIgnoreCase) ||
            contentType.StartsWith("application/vnd.ms-word", StringComparison.OrdinalIgnoreCase) ||
            contentType.StartsWith("application/vnd.openxmlformats-officedocument.", StringComparison.OrdinalIgnoreCase) ||
            contentType.StartsWith("application/vnd.oasis.opendocument.", StringComparison.OrdinalIgnoreCase))
            return Document;

        return Other;
    }
}
