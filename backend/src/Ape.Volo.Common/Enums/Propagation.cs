namespace Ape.Volo.Common.Enums;

/// <summary>
/// 事务传播行为。
/// 由 <c>TransactionAop</c> 在拦截时按此决定如何加入或创建事务。
/// </summary>
public enum Propagation
{
    /// <summary>
    /// 当前没有事务时新建事务，已有事务时加入当前事务。
    /// 默认值，适用于绝大多数业务方法。
    /// </summary>
    Required = 0,

    /// <summary>
    /// 必须加入当前事务；不存在事务时抛出异常。
    /// 用于「只能作为更大事务的一部分被调用」的方法，避免被误当成独立事务执行。
    /// </summary>
    Mandatory = 1,

    /// <summary>
    /// 已有事务时在当前事务内创建保存点，没有事务时新建根事务。
    /// 内层失败只回滚到保存点，不影响外层已完成的写入。
    /// </summary>
    Nested = 2
}
