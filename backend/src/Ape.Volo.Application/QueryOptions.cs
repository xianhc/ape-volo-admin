using System.Linq.Expressions;
using SqlSugar;

namespace Ape.Volo.Application;

/// <summary>
/// 查询操作对象
/// </summary>
/// <typeparam name="T"></typeparam>
public class QueryOptions<T>
{
    /// <summary>
    /// 分页
    /// </summary>
    public Pagination? Pagination { get; set; }

    /// <summary>
    /// 条件表达式
    /// </summary>
    public Expression<Func<T, bool>>? WhereLambda { get; set; }


    /// <summary>
    /// 条件模型
    /// </summary>
    public List<IConditionalModel>? ConditionalModels { get; set; }


    /// <summary>
    /// 查询表达式
    /// </summary>
    public Expression<Func<T, T>>? SelectExpression { get; set; }

    /// <summary>
    /// 是否分表
    /// </summary>
    public bool IsSplitTable { get; set; } = false;

    /// <summary>
    /// 未指定非空导航白名单时，是否自动加载全部一级导航属性。
    /// </summary>
    public bool IsIncludes { get; set; } = false;

    /// <summary>
    /// 显式加载的一级导航属性名，建议使用 nameof；非空时优先于自动加载及排除名单。
    /// null 或空数组沿用 IsIncludes 和 IgnorePropertyNameList。
    /// </summary>
    public string[]? IncludePropertyNameList { get; set; }

    /// <summary>
    /// 自动加载时忽略的一级导航属性名，仅在 IsIncludes 为 true 且未指定白名单时生效。
    /// </summary>
    public string[]? IgnorePropertyNameList { get; set; }

    /// <summary>
    /// 锁 分页查询默认NoLock
    /// </summary>
    public string LockString { get; set; } = SqlWith.NoLock;

    /// <summary>
    /// 缓存时间(秒)
    /// </summary>
    public int CacheDurationInSeconds { get; set; } = 0;
}
