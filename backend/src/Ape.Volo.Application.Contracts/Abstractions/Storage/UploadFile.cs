namespace Ape.Volo.Application.Contracts.Abstractions.Storage;

/// <summary>
/// 上传文件契约：应用层只认识该类型，不依赖宿主框架的 <c>IFormFile</c>。
/// 由宿主层负责把表单文件转换成本类型。
/// </summary>
public sealed class UploadFile : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// 原始文件名（含扩展名）
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// 内容类型
    /// </summary>
    public string ContentType { get; init; } = "application/octet-stream";

    /// <summary>
    /// 字节长度
    /// </summary>
    public long Length { get; init; }

    /// <summary>
    /// 文件内容流
    /// </summary>
    public required Stream Content { get; init; }

    /// <summary>
    /// 把内容写入目标流
    /// </summary>
    /// <param name="target">目标流</param>
    /// <param name="ct">取消令牌</param>
    public Task CopyToAsync(Stream target, CancellationToken ct = default) => Content.CopyToAsync(target, ct);

    /// <inheritdoc />
    public void Dispose() => Content.Dispose();

    /// <inheritdoc />
    public ValueTask DisposeAsync() => Content.DisposeAsync();
}
