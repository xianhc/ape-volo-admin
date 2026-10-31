

namespace Ape.Volo.Application.Contracts.Log.Models;

/// <summary>
/// 访问趋势
/// </summary>
public class VisitTrendVo
{
    /// <summary>
    /// 日期
    /// </summary>
    public List<string> DateList { get; set; } = new();

    /// <summary>
    /// 操作
    /// </summary>
    public List<int> OperateList { get; set; } = new();

    /// <summary>
    /// 异常
    /// </summary>
    public List<int> ExceptionList { get; set; } = new();
}
