using Ape.Volo.Common.Enums;
using SqlSugar;

namespace Ape.Volo.Core.Repository.UnitOfWork;

/// <summary>
/// 工作单元接口
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// 获取当前工作单元的数据库上下文
    /// </summary>
    SqlSugarScope GetDbClient();

    /// <summary>
    /// 当前是否处于事务中
    /// </summary>
    bool IsInTransaction { get; }

    /// <summary>
    /// 登记根事务成功提交后执行的动作；没有事务时立即执行。回滚会丢弃对应动作。
    /// 动作失败不回滚已提交数据，其他已登记动作仍会执行。
    /// </summary>
    /// <param name="action">不依赖请求取消令牌的提交后动作</param>
    /// <returns>登记或立即执行的任务</returns>
    Task AfterCommitAsync(Func<Task> action);

    /// <summary>
    /// 按传播行为开始事务（同步版本）。
    /// 必须在调用方上下文中同步开启：SqlSugar 的连接上下文存放在 AsyncLocal，
    /// 若在 async 方法内部开启，返回调用方后上下文会丢失，事务内的写入会落到另一条连接上。
    /// </summary>
    /// <param name="propagation">事务传播行为</param>
    /// <returns>事务作用域，使用 <c>await using</c> 释放</returns>
    ITransactionScope BeginTransaction(Propagation propagation);

}
