using Ape.Volo.Common.Extensions;
using Ape.Volo.Web.Export;
using Ape.Volo.Shared.Model;
using Microsoft.AspNetCore.Mvc;

namespace Ape.Volo.Web.Controllers.Base;

/// <summary>
/// 基控制器
/// </summary>
public class BaseController : Controller
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="vm"></param>
    /// <returns></returns>
    private ContentResult JsonContent(ActionResultVm vm)
    {
        return new ContentResult
        {
            Content = new ActionResultVm
            {
                Status = vm.Status,
                ActionError = vm.ActionError,
                Message = vm.Message,
                Timestamp = DateTime.Now.ToUnixTimeStampMillisecond().ToString(),
                Path = Request.Path.Value?.ToLower()!
            }.ToJson(),
            ContentType = "application/json; charset=utf-8",
            StatusCode = vm.Status
        };
    }


    /// <summary>
    /// 返回Json
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    protected ContentResult JsonContent(object? obj)
    {
        return new ContentResult
        {
            Content = obj.ToJson(),
            ContentType = "application/json; charset=utf-8",
            StatusCode = StatusCodes.Status200OK
        };
    }

    /// <summary>
    /// 返回Json  忽略空值字段
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    protected ContentResult JsonContentIgnoreNullValue(object? obj)
    {
        return new ContentResult
        {
            Content = obj.ToJsonByIgnore(),
            ContentType = "application/json; charset=utf-8",
            StatusCode = StatusCodes.Status200OK
        };
    }


    /// <summary>
    /// 返回响应
    /// </summary>
    /// <returns></returns>
    protected ActionResult Ok(OperateResult operateResult)
    {
        var msg = "请求成功";
        if (!operateResult.IsSuccess)
        {
            return Error(operateResult.Message);
        }

        if (Request.Method.ToUpper() == "POST")
        {
            // string pattern = @"/create$";
            // bool isMatch = Regex.IsMatch(Request.Path, pattern);
            // if (isMatch)
            // {
            //     return Created(msg);
            // }
            return Created();
        }

        if (Request.Method == "PUT")
        {
            // string pattern = @"/edit$";
            // bool isMatch = Regex.IsMatch(Request.Path, pattern);
            // if (isMatch)
            // {
            //     return new NoContentResult();
            // }
            return new NoContentResult();
        }

        if (Request.Method == "DELETE")
        {
            // string pattern = @"/delete$";
            // bool isMatch = Regex.IsMatch(Request.Path, pattern);
            // if (isMatch)
            // {
            //     msg = "删除成功";
            // }
            msg = "删除成功";
        }

        return Success(msg);
    }


    /// <summary>
    /// 返回成功
    /// </summary>
    /// <param name="msg">消息</param>
    /// <returns></returns>
    private ContentResult Success(string msg = "")
    {
        msg = msg.IsNullOrEmpty() ? "请求成功" : msg;
        var vm = new ActionResultVm
        {
            Status = StatusCodes.Status200OK,
            Message = msg
        };

        return JsonContent(vm);
    }

    /// <summary>
    /// 创建成功
    /// </summary>
    /// <returns></returns>
    private ContentResult Created(string msg = "")
    {
        msg = msg.IsNullOrEmpty() ? "创建成功" : msg;
        var vm = new ActionResultVm
        {
            Status = StatusCodes.Status201Created,
            Message = msg
        };

        return JsonContent(vm);
    }

    /// <summary>
    /// 把导出数据生成 Excel 文件结果
    /// </summary>
    /// <param name="exports">导出数据</param>
    /// <param name="titlePrefix">文件名前缀，如「用户」「部门」</param>
    /// <returns>Excel 文件结果</returns>
    protected FileContentResult ExportExcel(List<ExportBase> exports, string titlePrefix)
    {
        var data = new ExcelHelper().GenerateExcel(exports, out var mimeType, out var fileName);
        return new FileContentResult(data, mimeType) { FileDownloadName = titlePrefix + fileName };
    }

    /// <summary>
    /// 返回错误
    /// </summary>
    /// <param name="msg">错误提示</param>
    /// <returns></returns>
    protected ContentResult Error(string msg = "")
    {
        msg = msg.IsNullOrEmpty()
            ? "请求失败"
            : msg;
        var vm = new ActionResultVm
        {
            Status = StatusCodes.Status400BadRequest,
            Message = msg,
            ActionError = new ActionError()
        };

        return JsonContent(vm);
    }

    /// <summary>
    /// 返回错误
    /// </summary>
    /// <param name="actionError">错误集合</param>
    /// <returns></returns>
    protected ContentResult Error(ActionError actionError)
    {
        var vm = new ActionResultVm
        {
            Status = StatusCodes.Status400BadRequest,
            ActionError = actionError,
            Message = actionError.GetFirstError()
        };

        return JsonContent(vm);
    }
}
