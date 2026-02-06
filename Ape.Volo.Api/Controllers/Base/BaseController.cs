using System;
using System.Collections.Generic;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Model;
using Ape.Volo.Core;
using Ape.Volo.SharedModel.Queries.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ape.Volo.Api.Controllers.Base;

/// <summary>
/// 基控制器
/// </summary>
//[JsonParamter]
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
                Path = Request.Path.Value?.ToLower()
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
    protected ContentResult JsonContent(object obj)
    {
        return new ContentResult
        {
            Content = obj.ToJson(),
            ContentType = "application/json; charset=utf-8",
            StatusCode = StatusCodes.Status200OK
        };
    }

    /// <summary>
    /// 返回Json
    /// </summary>
    /// <param name="content"></param>
    /// <param name="pagination"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    protected ContentResult JsonContent<T>(List<T> content, Pagination pagination) where T : class, new()
    {
        var result = new ActionResultVm<T>
        {
            Content = content,
            TotalElements = pagination.TotalElements,
            TotalPages = pagination.TotalPages
        };

        return new ContentResult
        {
            Content = result.ToJson(),
            ContentType = "application/json; charset=utf-8",
            StatusCode = StatusCodes.Status200OK
        };
    }


    /// <summary>
    /// 返回Json  忽略空值字段
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    protected ContentResult JsonContentIgnoreNullValue(object obj)
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
        var msg = App.L.R("Sys.HttpOK");
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
            //     msg = App.L.R("Sys.HttpDelete");
            // }
            msg = App.L.R("Sys.HttpDelete");
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
        msg = msg.IsNullOrEmpty() ? App.L.R("Sys.HttpOK") : msg;
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
        msg = msg.IsNullOrEmpty() ? App.L.R("Sys.HttpCreated") : msg;
        var vm = new ActionResultVm
        {
            Status = StatusCodes.Status201Created,
            Message = msg
        };

        return JsonContent(vm);
    }

    /// <summary>
    /// 返回错误
    /// </summary>
    /// <param name="msg">错误提示</param>
    /// <returns></returns>
    protected ContentResult Error(string msg = "")
    {
        msg = msg.IsNullOrEmpty()
            ? App.L.R("Sys.HttpBadRequest")
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
