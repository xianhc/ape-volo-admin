namespace Ape.Volo.Application.Contracts.Abstractions.Caching;

/// <summary>可选的缓存键枚举能力，仅由支持枚举的存储实现。</summary>
public interface ICacheKeyCatalog
{
    /// <summary>枚举指定前缀的缓存键。</summary>
    /// <param name="prefix">键前缀。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>当前可见的键快照。</returns>
    Task<string[]> GetKeysAsync(string prefix, CancellationToken ct = default);
}
