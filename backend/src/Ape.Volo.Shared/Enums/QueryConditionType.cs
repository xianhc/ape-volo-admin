namespace Ape.Volo.Shared.Enums;

/// <summary>
/// 查询条件类型。
/// 契约层自有枚举，不依赖具体 ORM；由基础设施层映射到实际查询表达式。
/// </summary>
public enum QueryConditionType
{
    /// <summary>
    /// 等于
    /// </summary>
    Equal,

    /// <summary>
    /// 不等于
    /// </summary>
    NotEqual,

    /// <summary>
    /// 包含（模糊匹配）
    /// </summary>
    Like,

    /// <summary>
    /// 左匹配
    /// </summary>
    LikeLeft,

    /// <summary>
    /// 右匹配
    /// </summary>
    LikeRight,

    /// <summary>
    /// 不包含
    /// </summary>
    NotLike,

    /// <summary>
    /// 大于
    /// </summary>
    GreaterThan,

    /// <summary>
    /// 大于等于
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// 小于
    /// </summary>
    LessThan,

    /// <summary>
    /// 小于等于
    /// </summary>
    LessThanOrEqual,

    /// <summary>
    /// 在集合内
    /// </summary>
    In,

    /// <summary>
    /// 不在集合内
    /// </summary>
    NotIn,

    /// <summary>
    /// 为空
    /// </summary>
    IsNull,

    /// <summary>
    /// 不为空
    /// </summary>
    IsNotNull
}
