using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.Diagnostics;

/// <summary>前端诊断摘要；不接收异常正文、堆栈或请求内容。</summary>
public sealed class ClientErrorReportDto
{
    /// <summary>固定错误事件码。</summary>
    [Required, RegularExpression("^[a-z][a-z0-9.-]{0,63}$")]
    public string Code { get; set; } = string.Empty;

    /// <summary>错误类别名称。</summary>
    [Required, RegularExpression("^[a-zA-Z0-9_.:-]{1,64}$")]
    public string ErrorName { get; set; } = string.Empty;

    /// <summary>路由名称，不包含 URL、查询参数或页面数据。</summary>
    [RegularExpression("^[a-zA-Z0-9_.:-]{0,100}$")]
    public string RouteName { get; set; } = string.Empty;

    /// <summary>前端构建标识。</summary>
    [RegularExpression("^[a-zA-Z0-9_.:-]{0,64}$")]
    public string Build { get; set; } = string.Empty;

    /// <summary>关联后端响应的 X-Request-ID。</summary>
    [RegularExpression("^[a-zA-Z0-9_.:-]{0,128}$")]
    public string RequestId { get; set; } = string.Empty;
}
