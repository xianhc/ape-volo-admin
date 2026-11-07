using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Extensions;
using System.Reflection;
using System.Globalization;
using Ape.Volo.Core.Caches;
using Ape.Volo.Core.Repository.UnitOfWork;
using Castle.DynamicProxy;

namespace Ape.Volo.Core.Aop;

/// <summary>
/// 缓存拦截器：保留同步方法支持，Task 方法通过异步适配器执行。
/// </summary>
public class CacheAop : IInterceptor, IAsyncInterceptor
{
    private readonly ICache _cache;
    private readonly AsyncDeterminationInterceptor _dispatcher;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// 初始化 <see cref="CacheAop"/> 的新实例。
    /// </summary>
    /// <param name="cache">cache</param>
    /// <param name="unitOfWork">事务状态。</param>

    public CacheAop(ICache cache, IUnitOfWork unitOfWork)
    {
        _cache = cache;
        _unitOfWork = unitOfWork;
        _dispatcher = new AsyncDeterminationInterceptor(this);
    }

    public void Intercept(IInvocation invocation)
    {
        _dispatcher.Intercept(invocation);
    }

    public void InterceptSynchronous(IInvocation invocation)
    {
        var attribute = GetCacheAttribute(invocation);
        if (attribute == null || invocation.Method.ReturnType == typeof(void))
        {
            invocation.Proceed();
            return;
        }

        var cacheKey = CreateCacheKey(invocation, attribute);
        var cached = _cache.Get<string>(cacheKey);
        if (cached != null)
        {
            invocation.ReturnValue = invocation.Method.ReturnType == typeof(string)
                ? cached : cached.ToObject(invocation.Method.ReturnType);
            return;
        }

        invocation.Proceed();
        if (!invocation.ReturnValue.IsNullOrEmpty())
        {
            _cache.Set(cacheKey, invocation.ReturnValue, TimeSpan.FromMinutes(attribute.Expiration), null);
        }
    }

    public void InterceptAsynchronous(IInvocation invocation)
    {
        // 没有返回值的异步方法不参与结果缓存。
        invocation.Proceed();
    }

    public void InterceptAsynchronous<TResult>(IInvocation invocation)
    {
        var attribute = GetCacheAttribute(invocation);
        if (attribute == null)
        {
            invocation.Proceed();
            return;
        }

        // await 后再 Proceed 时必须保留当前拦截器在调用链中的位置。
        var proceedInfo = invocation.CaptureProceedInfo();
        invocation.ReturnValue = InterceptAsync<TResult>(invocation, proceedInfo, attribute);
    }

    private async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, IInvocationProceedInfo proceedInfo,
        UseCacheAttribute attribute)
    {
        var ct = invocation.Arguments.OfType<CancellationToken>().FirstOrDefault();
        ct.ThrowIfCancellationRequested();
        var cacheKey = CreateCacheKey(invocation, attribute);
        async Task<TResult> Load()
        {
            proceedInfo.Invoke();
            return await (Task<TResult>)invocation.ReturnValue;
        }
        if (_unitOfWork.IsInTransaction) return await Load();
        return await _cache.GetOrCreateAsync(cacheKey, Load, TimeSpan.FromMinutes(attribute.Expiration), ct);
    }

    private static UseCacheAttribute? GetCacheAttribute(IInvocation invocation)
    {
        return (invocation.MethodInvocationTarget ?? invocation.Method)
            .GetCustomAttribute<UseCacheAttribute>(inherit: true);
    }

    /// <summary>
    /// 构建Redis Key
    /// </summary>
    /// <param name="invocation"></param>
    /// <param name="useCacheAttribute"></param>
    /// <returns></returns>
    private static string CreateCacheKey(IInvocation invocation, UseCacheAttribute useCacheAttribute)
    {
        var typeName = invocation.TargetType.Name;
        var methodName = invocation.Method.Name;

        // 简单值直接拼接，复杂查询参数序列化后计算摘要。
        var methodArguments = invocation.Arguments.Where(arg => arg is not CancellationToken)
            .Select(GetArgumentValue).ToList();

        var key = useCacheAttribute.KeyPrefix.IsNullOrEmpty()
            ? $"{typeName}:{methodName}:"
            : useCacheAttribute.KeyPrefix;

        methodArguments.ForEach(arg => { key = $"{key}{arg}:"; });

        return key.TrimEnd(':');
    }

    /// <summary>
    /// 获取参数值类型值
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    private static string GetArgumentValue(object? arg)
    {
        if (arg == null) return string.Empty;

        switch (arg)
        {
            case string text:
                return text;
            case DateTime date:
                return date.ToString("O", CultureInfo.InvariantCulture);
            case DateTimeOffset date:
                return date.ToString("O", CultureInfo.InvariantCulture);
            case ValueType when arg.GetType().IsPrimitive || arg is decimal or Guid or Enum:
                return Convert.ToString(arg, CultureInfo.InvariantCulture) ?? string.Empty;
            default:
            {
                return arg.ToJson().ToMd5String16();
            }
        }
    }
}
