using Ape.Volo.Shared.Model;
using Ape.Volo.Web.Export;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ape.Volo.Web.DynamicApi;

/// <summary>
/// 动态接口导出过滤器：把 <c>List&lt;ExportBase&gt;</c> 返回值转成 Excel 文件
/// </summary>
public sealed class DynamicApiExportFilter : IAlwaysRunResultFilter
{
    /// <summary>
    /// 返回值是导出模型集合时替换为文件结果
    /// </summary>
    /// <param name="context">结果执行上下文</param>
    public void OnResultExecuting(ResultExecutingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Result is not ObjectResult { Value: List<ExportBase> exportRows } || exportRows.Count == 0)
        {
            return;
        }

        var data = new ExcelHelper().GenerateExcel(exportRows, out var mimeType, out var fileName);
        var prefix = context.ActionDescriptor is ControllerActionDescriptor descriptor
            ? descriptor.ControllerName
            : "export";

        context.Result = new FileContentResult(data, mimeType) { FileDownloadName = prefix + fileName };
    }

    /// <summary>
    /// 结果执行完毕后无需处理
    /// </summary>
    /// <param name="context">结果执行上下文</param>
    public void OnResultExecuted(ResultExecutedContext context)
    {
    }
}
