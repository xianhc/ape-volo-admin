namespace Ape.Volo.Web.Extensions;

/// <summary>
/// 表单文件到应用层契约的转换
/// </summary>
public static class UploadFileExtensions
{
    /// <summary>
    /// 把 <see cref="IFormFile"/> 转换为应用层使用的 <see cref="UploadFile"/>。
    /// 调用方负责释放返回值（流由表单文件持有）。
    /// </summary>
    /// <param name="file">表单文件</param>
    /// <returns>上传文件契约</returns>
    public static UploadFile ToUploadFile(this IFormFile file)
    {
        return new UploadFile
        {
            FileName = file.FileName,
            ContentType = file.ContentType,
            Length = file.Length,
            Content = file.OpenReadStream()
        };
    }
}
