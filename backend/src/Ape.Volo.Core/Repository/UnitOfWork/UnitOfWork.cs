using Ape.Volo.Common.Enums;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace Ape.Volo.Core.Repository.UnitOfWork;

/// <summary>
/// 工作单元实现。
/// 事务状态保存在 <see cref="AsyncLocal{T}"/> 中，按传播行为区分「根事务 / 加入者 / 保存点」。
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly ILogger<UnitOfWork> _logger;

    /// <summary>
    /// 环境事务容器。
    /// 必须在同步入口赋值——异步方法内部替换 AsyncLocal.Value 不会回传到调用方。
    /// </summary>
    private readonly AsyncLocal<AmbientTransaction?> _ambient = new();

    /// <summary>
    /// 初始化 <see cref="UnitOfWork"/> 的新实例。
    /// </summary>
    /// <param name="sqlSugarClient">sqlSugarClient</param>
    /// <param name="logger">logger</param>
    public UnitOfWork(ISqlSugarClient sqlSugarClient, ILogger<UnitOfWork> logger)
    {
        _sqlSugarClient = sqlSugarClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public bool IsInTransaction => _ambient.Value?.Current is not null;

    /// <inheritdoc />
    public Task AfterCommitAsync(Func<Task> action)
    {
        var state = _ambient.Value?.Current;
        if (state == null) return action();
        state.AfterCommitActions.Add(action);
        return Task.CompletedTask;
    }

    private async Task RunAfterCommitAsync(TransactionState state)
    {
        var actions = state.AfterCommitActions.ToArray();
        state.AfterCommitActions.Clear();
        List<Exception> failures = [];
        foreach (var action in actions)
        {
            try { await action(); }
            catch (Exception exception)
            {
                failures.Add(exception);
                _logger.LogError(exception, "数据库已提交，提交后动作执行失败。");
            }
        }
        if (failures.Count != 0)
            throw new AggregateException("数据库已提交，但提交后动作执行失败。", failures);
    }

    /// <summary>
    /// 获取DB，保证唯一性
    /// </summary>
    /// <returns></returns>
    public SqlSugarScope GetDbClient()
    {
        return (_sqlSugarClient as SqlSugarScope)!;
    }

    /// <inheritdoc />
    public ITransactionScope BeginTransaction(Propagation propagation)
    {
        if (!Enum.IsDefined(propagation))
        {
            throw new ArgumentOutOfRangeException(nameof(propagation), propagation, "不支持的事务传播行为。");
        }

        var ambient = _ambient.Value ??= new AmbientTransaction();
        var current = ambient.Current;

        if (propagation == Propagation.Mandatory && current is null)
        {
            throw new InvalidOperationException("事务传播行为为 Mandatory，但当前没有活动事务。");
        }

        // Required / Mandatory：已有事务时加入，不真正开启新事务
        if (current is not null && propagation is Propagation.Required or Propagation.Mandatory)
        {
            return new TransactionScope(this, ambient, current, ownsTransaction: false, savepointName: null);
        }

        // Nested：已有事务时创建保存点，内层失败只回滚到保存点
        if (current is not null && propagation == Propagation.Nested)
        {
            var savepointName = $"sp_{Interlocked.Increment(ref current.SavepointSequence)}";
            try
            {
                GetDbClient().Ado.ExecuteCommand(BuildSavepointSql(savepointName));
            }
            catch
            {
                // 保存点创建失败后，调用方即使捕获异常也不能提交外层事务
                current.RollbackOnly = true;
                throw;
            }

            return new TransactionScope(this, ambient, current, ownsTransaction: false, savepointName);
        }

        // 没有事务：Required / Nested 都新建根事务
        GetDbClient().Ado.BeginTran();
        var state = new TransactionState();
        ambient.Current = state;
        _logger.LogDebug("Started transaction with propagation {Propagation}.", propagation);
        return new TransactionScope(this, ambient, state, ownsTransaction: true, savepointName: null);
    }

    /// <summary>
    /// 构造创建保存点的 SQL（SQL Server 与其它方言不同）
    /// </summary>
    private string BuildSavepointSql(string savepointName)
    {
        return GetDbClient().CurrentConnectionConfig.DbType == DbType.SqlServer
            ? $"SAVE TRANSACTION {savepointName}"
            : $"SAVEPOINT {savepointName}";
    }

    /// <summary>
    /// 构造回滚到保存点的 SQL
    /// </summary>
    private string BuildRollbackToSavepointSql(string savepointName)
    {
        return GetDbClient().CurrentConnectionConfig.DbType == DbType.SqlServer
            ? $"ROLLBACK TRANSACTION {savepointName}"
            : $"ROLLBACK TO SAVEPOINT {savepointName}";
    }

    /// <summary>
    /// 执行保存点语句
    /// </summary>
    private async Task ExecuteSavepointCommandAsync(string sql)
    {
        await GetDbClient().Ado.ExecuteCommandAsync(sql);
    }

    /// <summary>
    /// 清理当前异步流中的环境事务
    /// </summary>
    private void ClearAmbient(AmbientTransaction ambient, TransactionState state)
    {
        if (ReferenceEquals(ambient.Current, state))
        {
            ambient.Current = null;
        }
    }

    /// <summary>在当前连接同步开启底层事务。</summary>
    private void BeginTran()
    {
        // 必须走 Ado 而不是 scope 级 API：SqlSugarScope 的 BeginTran 会在默认连接上开启事务，
        // 与实体查询实际使用的连接不是同一个，导致事务内的写入被自己锁住（SQLite 单写者）。
        GetDbClient().Ado.BeginTran();
    }

    /// <summary>提交当前底层事务。</summary>
    private void CommitTran()
    {
        try
        {
            GetDbClient().Ado.CommitTran();
        }
        catch (Exception ex)
        {
            GetDbClient().Ado.RollbackTran();
            _logger.LogCritical(ex.Message);
            throw;
        }
    }

    /// <summary>回滚当前底层事务。</summary>
    private void RollbackTran()
    {
        GetDbClient().Ado.RollbackTran();
    }

    /// <summary>
    /// 环境事务容器：嵌套调用共享同一个实例
    /// </summary>
    private sealed class AmbientTransaction
    {
        /// <summary>
        /// 当前根事务状态；为 null 表示不在事务中
        /// </summary>
        public TransactionState? Current { get; set; }
    }

    /// <summary>
    /// 根事务状态
    /// </summary>
    private sealed class TransactionState
    {
        /// <summary>
        /// 是否被内层标记为「只能回滚」
        /// </summary>
        public bool RollbackOnly { get; set; }

        /// <summary>
        /// 保存点序号
        /// </summary>
        public int SavepointSequence;

        public List<Func<Task>> AfterCommitActions { get; } = [];
    }

    /// <summary>
    /// 事务作用域实现：根事务负责提交/回滚，加入者只标记状态，保存点回滚到检查点。
    /// </summary>
    private sealed class TransactionScope : ITransactionScope
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly AmbientTransaction _ambient;
        private readonly TransactionState _state;
        private readonly bool _ownsTransaction;
        private readonly string? _savepointName;
        private readonly int _afterCommitCheckpoint;
        private bool _completed;

        /// <summary>
        /// 初始化事务作用域
        /// </summary>
        public TransactionScope(UnitOfWork unitOfWork, AmbientTransaction ambient, TransactionState state,
            bool ownsTransaction, string? savepointName)
        {
            _unitOfWork = unitOfWork;
            _ambient = ambient;
            _state = state;
            _ownsTransaction = ownsTransaction;
            _savepointName = savepointName;
            _afterCommitCheckpoint = state.AfterCommitActions.Count;
        }

        /// <inheritdoc />
        public Task CommitAsync(CancellationToken ct = default)
        {
            if (_completed)
            {
                return Task.CompletedTask;
            }

            // 保存点：不显式释放（Oracle 不支持 RELEASE SAVEPOINT，事务结束会自动清理）
            if (_savepointName is not null)
            {
                _completed = true;
                return Task.CompletedTask;
            }

            // 加入者不能提交外层事务
            if (!_ownsTransaction)
            {
                _completed = true;
                return Task.CompletedTask;
            }

            if (_state.RollbackOnly)
            {
                try
                {
                    _unitOfWork.RollbackTran();
                }
                finally
                {
                    _completed = true;
                    _state.AfterCommitActions.Clear();
                    _unitOfWork.ClearAmbient(_ambient, _state);
                }

                throw new InvalidOperationException("事务已被内层操作标记为只能回滚，拒绝提交。");
            }

            _unitOfWork.CommitTran();
            _completed = true;
            _unitOfWork.ClearAmbient(_ambient, _state);
            return _unitOfWork.RunAfterCommitAsync(_state);
        }

        /// <inheritdoc />
        public async Task RollbackAsync(CancellationToken ct = default)
        {
            if (_completed)
            {
                return;
            }

            // 保存点：回滚到检查点，外层已完成的写入保留
            if (_savepointName is not null)
            {
                try
                {
                    await _unitOfWork.ExecuteSavepointCommandAsync(
                        _unitOfWork.BuildRollbackToSavepointSql(_savepointName));
                }
                catch
                {
                    // 保存点回滚失败后，外层事务不再允许提交
                    _state.RollbackOnly = true;
                    throw;
                }
                finally
                {
                    _completed = true;
                    _state.AfterCommitActions.RemoveRange(_afterCommitCheckpoint,
                        _state.AfterCommitActions.Count - _afterCommitCheckpoint);
                }

                return;
            }

            // 加入者不能回滚外层事务，只能让根作用域最终回滚
            if (!_ownsTransaction)
            {
                _state.RollbackOnly = true;
                _completed = true;
                return;
            }

            try
            {
                _unitOfWork.RollbackTran();
            }
            finally
            {
                _completed = true;
                _state.AfterCommitActions.Clear();
                _unitOfWork.ClearAmbient(_ambient, _state);
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (!_completed)
            {
                // 调用方忘记提交/回滚时兜底回滚，避免数据库留下活动事务
                await RollbackAsync(CancellationToken.None);
            }
        }
    }
}
