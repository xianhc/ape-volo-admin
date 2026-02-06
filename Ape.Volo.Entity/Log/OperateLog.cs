using System;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Base;
using SqlSugar;

namespace Ape.Volo.Entity.Log
{
    /// <summary>
    /// 操作日志
    /// </summary>
    [LogDataBase]
    [SplitTable(SplitType.Month)]
    [SugarTable($@"{"log_operate"}_{{year}}{{month}}{{day}}", IsDisabledUpdateAll = true)]
    // 添加复合索引：按创建人 + 创建时间范围查询
    // [SugarIndex("index_{split_table}_CreateBy_CreateTime", nameof(CreateBy), OrderByType.Asc, nameof(CreateTime),
    //     OrderByType.Desc)]
    // 添加单字段索引：按操作类型（Controller + Action）查询
    [SugarIndex("index_{split_table}_Controller", nameof(Controller), OrderByType.Asc)]
    [SugarIndex("index_{split_table}_Action", nameof(Action), OrderByType.Asc)]
    // 添加慢查询索引：执行耗时超过阈值
    [SugarIndex("index_{split_table}_ExecutionDuration", nameof(ExecutionDuration), OrderByType.Desc)]
    // 添加 IP 查询索引
    [SugarIndex("index_{split_table}_RequestIp", nameof(RequestIp), OrderByType.Asc)]
    public class OperateLog : BaseEntity
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
        /// /描述
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
        /// 响应数据
        /// </summary>
        [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString)]
        public string? ResponseData { get; set; }

        /// <summary>
        /// 执行耗时(毫秒)
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public long ExecutionDuration { get; set; }

        /// <summary>
        /// 请求IP
        /// </summary>
        public string? RequestIp { get; set; }

        /// <summary>
        /// IP所属真实地址
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
        /// 创建时间
        /// </summary>
        [SplitField]
        public new DateTime CreateTime { get; set; }
    }
}