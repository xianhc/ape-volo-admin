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
    /// 是否分表
    /// </summary>
    public bool IsSplitTable { get; set; } = false;

    /// <summary>
    /// 显式加载的一级导航属性名，建议使用 nameof；null 或空数组不加载导航。
    /// </summary>
    public string[]? IncludePropertyNameList { get; set; }

}
