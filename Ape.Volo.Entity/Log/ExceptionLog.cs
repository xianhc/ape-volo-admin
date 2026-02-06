using System;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Enums;
using Ape.Volo.Entity.Base;
using SqlSugar;

namespace Ape.Volo.Entity.Log
{
    /// <summary>
    /// 异常日志
    /// </summary>
    [LogDataBase]
    [SplitTable(SplitType.Month)]
    [SugarTable($@"{"log_exception"}_{{year}}{{month}}{{day}}", IsDisabledUpdateAll = true)]
    public class ExceptionLog : BaseEntity
    {
        /// <summary>
        /// 区域
        /// </summary>
        public string? Area { get; set; }

        /// <summary>
        /// 控制器
        /// </summary>
        public string? Controller { get; set; }

        /// <summary>
        /// 方法
        /// </summary>
        public string? Action { get; set; }

        /// <summary>
        /// 请求方式
        /// </summary>
        public string? Method { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 请求url
        /// </summary>
        public string? RequestUrl { get; set; }

        /// <summary>
        /// 请求参数
        /// </summary>
        [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString)]
        public string? RequestParameters { get; set; }

        /// <summary>
        /// 异常短信息
        /// </summary>
        public string? ExceptionMessage { get; set; }

        /// <summary>
        /// 异常完整信息
        /// </summary>
        [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString)]
        public string? ExceptionMessageFull { get; set; }

        /// <summary>
        /// 异常堆栈信息
        /// </summary>
        [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString)]
        public string? ExceptionStack { get; set; }

        /// <summary>
        /// 等级
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// 请求ip
        /// </summary>
        public string? RequestIp { get; set; }

        /// <summary>
        /// ip所属真实地址
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// 用户代理信息
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// 操作系统
        /// </summary>
        public string? OperatingSystem { get; set; }

        /// <summary>
        /// 设备类型
        /// </summary>
        public string? DeviceType { get; set; }

        /// <summary>
        /// 浏览器名称
        /// </summary>
        public string? BrowserName { get; set; }

        /// <summary>
        /// 浏览器版本
        /// </summary>
        public string? Version { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [SplitField]
        public new DateTime CreateTime { get; set; }
    }
}
