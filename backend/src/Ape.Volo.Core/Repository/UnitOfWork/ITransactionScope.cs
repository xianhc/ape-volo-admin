namespace Ape.Volo.Core.Repository.UnitOfWork;

/// <summary>
/// 一次事务作用域：由 <see cref="IUnitOfWork.BeginTransaction"/> 创建，
/// 按传播行为决定它是根事务、加入者还是保存点。
/// </summary>
public interface ITransactionScope : IAsyncDisposable
{
    /// <summary>
    /// 提交当前作用域。加入者（Required/Mandatory）不会真正提交外层事务。
    /// </summary>
    /// <param name="ct">取消令牌</param>
    Task CommitAsync(CancellationToken ct = default);

    /// <summary>
    /// 回滚当前作用域。加入者会把外层事务标记为「只能回滚」。
    /// </summary>
    /// <param name="ct">取消令牌</param>
    Task RollbackAsync(CancellationToken ct = default);
}
