using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Shared.Enums;

/// <summary>
/// 数据权限范围类型
/// </summary>
public enum DataScopeType
{
    /// <summary>
    /// 无
    /// </summary>
    [Display(Name = "无")]
    None = 0,

    /// <summary>
    /// 全部
    /// </summary>
    [Display(Name = "全部")]
    All = 1,

    /// <summary>
    /// 本人
    /// </summary>
    [Display(Name = "本人")]
    MySelf = 2,

    /// <summary>
    /// 本部门
    /// </summary>
    [Display(Name = "本部门")]
    MyDept = 3,

    /// <summary>
    /// 本部门及以下
    /// </summary>
    [Display(Name = "本部门及以下")]
    MyDeptAndBelow = 4,

    /// <summary>
    /// 自定义
    /// </summary>
    [Display(Name = "自定义")]
    Customize = 5
}
