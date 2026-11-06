

namespace Ape.Volo.Application.Contracts.Abstractions;

/// <summary>
/// 生成分布式唯一的长整型 ID。
/// </summary>
public interface IIdGenerator
{
    /// <summary>
    /// 生成一个新的 ID；实现须支持并发调用。
    /// </summary>
    /// <returns></returns>
    long NewId();
}
