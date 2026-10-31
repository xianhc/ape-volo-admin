using Ape.Volo.Shared.Enums;

namespace Ape.Volo.Application.Contracts.Abstractions.Caching;

/// <summary>应用使用的缓存存储，不暴露具体缓存服务器连接。</summary>
public interface ICache
{
    /// <summary>
    /// 原子比较并替换字符串值。expected 为 null 表示仅当键不存在时创建；value 为 null 表示删除。
    /// 写入使用绝对有效期，不会因读取而延长。调用方必须统一使用此方法修改需要原子性的键。
    /// </summary>
    Task<bool> CompareExchangeAsync(string key, string? expected, string? value, TimeSpan timeout,
        CancellationToken ct = default);

    #region 获取缓存

    /// <summary>
    /// 获取缓存数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    T? Get<T>(string key);

    /// <summary>
    /// 获取缓存数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="ct">取消令牌。</param>
    /// <returns></returns>
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);

    #endregion

    #region 添加缓存

    /// <summary>
    /// 添加缓存
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <param name="timeout">过期时间</param>
    /// <param name="cacheExpireType">过期类型</param>
    /// <returns>添加结果</returns>
    bool Set(string key, object value, TimeSpan? timeout, CacheExpireType? cacheExpireType);

    /// <summary>
    /// 添加缓存
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <param name="timeout">过期时间</param>
    /// <param name="cacheExpireType">过期类型</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>添加结果</returns>
    Task<bool> SetAsync(string key, object value, TimeSpan? timeout, CacheExpireType? cacheExpireType, CancellationToken ct = default);

    #endregion

    #region 移除缓存

    /// <summary>
    /// 移除缓存
    /// </summary>
    /// <param name="key">键</param>
    /// <returns>移除结果</returns>
    bool Remove(string key);

    /// <summary>
    /// 移除缓存
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>移除结果</returns>
    Task<bool> RemoveAsync(string key, CancellationToken ct = default);

    #endregion

}
