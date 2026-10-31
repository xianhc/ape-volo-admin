using System.ComponentModel;
using Ape.Volo.Web.Controllers.Base;
using Ape.Volo.Common.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Ape.Volo.Web.Controllers.SystemManagement;

/// <summary>
/// 文件上传：<c>IFormFile</c> 只能出现在宿主层，其余动作已由动态接口承担
/// </summary>
[Area("文件存储管理")]
[Route("/api/file-record", Order = 12)]
public class FileRecordController : BaseApiController
{
    #region 字段

    private readonly IFileRecordService _fileRecordService;

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化 <see cref="FileRecordController"/> 的新实例。
    /// </summary>
    /// <param name="fileRecordService">fileRecordService</param>
    public FileRecordController(IFileRecordService fileRecordService)
    {
        _fileRecordService = fileRecordService;
    }

    #endregion

    /// <summary>
    /// 新增文件
    /// </summary>
    /// <param name="file">上传的文件</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>操作结果</returns>
    [HttpPost("create")]
    [Description("执行创建")]
    public async Task<ActionResult> Create([FromForm] IFormFile file, CancellationToken ct)
    {
        if (file.IsNull())
        {
            return Error("file 是必填项");
        }

        await using var uploadFile = file.ToUploadFile();
        var result = await _fileRecordService.CreateAsync(uploadFile, ct);
        return Ok(result);
    }
}
