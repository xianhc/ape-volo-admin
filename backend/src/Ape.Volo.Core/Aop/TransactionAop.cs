using System.Reflection;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Core.Repository.UnitOfWork;
using Castle.DynamicProxy;

namespace Ape.Volo.Core.Aop;

/// <summary>
/// 事务拦截器。
/// 基于 <see cref="AsyncInterceptorBase"/>，异步方法全程 <c>await</c>，不再同步等待 Task。
/// 事务边界由 <see cref="IUnitOfWork.BeginTransaction"/> 在调用方上下文中同步创建。
/// </summary>
/// <remarks>
/// <see cref="AsyncInterceptorBase"/> 不拦截同步方法（其 <c>InterceptSynchronous</c> 不可重写），
/// 因此 <see cref="UseTranAttribute"/> 只能标注在返回 <see cref="Task"/> 的方法上——
/// 该约定由 <c>TransactionAopTests</c> 守护。
/// </remarks>
public class TransactionAop : AsyncInterceptorBase
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// 初始化 <see cref="TransactionAop"/> 的新实例。
    /// </summary>
    /// <param name="unitOfWork">unitOfWork</param>
    public TransactionAop(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 无返回值异步方法拦截
    /// </summary>
    /// <param name="invocation">拦截调用信息</param>
    /// <param name="proceedInfo">继续执行所需的信息</param>
    /// <param name="proceed">继续执行的委托</param>
    protected override async Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo,
        Func<IInvocation, IInvocationProceedInfo, Task> proceed)
    {
        var attribute = GetTransactionAttribute(invocation);
        if (attribute is null)
        {
            await proceed(invocation, proceedInfo);
            return;
        }

        await using var scope = _unitOfWork.BeginTransaction(attribute.Propagation);
        try
        {
            await proceed(invocation, proceedInfo);
            await scope.CommitAsync();
        }
        catch
        {
            // 回滚不使用请求取消令牌，尽量保证数据库不留下活动事务
            await scope.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    /// <summary>
    /// 有返回值异步方法拦截
    /// </summary>
    /// <typeparam name="TResult">返回值类型</typeparam>
    /// <param name="invocation">拦截调用信息</param>
    /// <param name="proceedInfo">继续执行所需的信息</param>
    /// <param name="proceed">继续执行的委托</param>
    /// <returns>方法返回值</returns>
    protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation,
        IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
    {
        var attribute = GetTransactionAttribute(invocation);
        if (attribute is null)
        {
            return await proceed(invocation, proceedInfo);
        }

        await using var scope = _unitOfWork.BeginTransaction(attribute.Propagation);
        try
        {
            var result = await proceed(invocation, proceedInfo);
            await scope.CommitAsync();
            return result;
        }
        catch
        {
            await scope.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    /// <summary>
    /// 读取方法上的事务特性；未标注时返回 null
    /// </summary>
    private static UseTranAttribute? GetTransactionAttribute(IInvocation invocation)
    {
        var method = invocation.MethodInvocationTarget ?? invocation.Method;
        return method.GetCustomAttribute<UseTranAttribute>(inherit: true);
    }
}
