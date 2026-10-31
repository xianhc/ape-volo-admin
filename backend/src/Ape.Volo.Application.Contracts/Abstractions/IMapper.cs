namespace Ape.Volo.Application.Contracts.Abstractions;

/// <summary>
/// 不依赖映射框架的应用映射契约。
/// </summary>
public interface IMapper
{
    /// <summary>
    /// 映射运行时类型的源对象。
    /// </summary>
    /// <param name="source">运行时类型的源对象</param>
    TDestination MapTo<TDestination>(object source);
    /// <summary>
    /// 映射指定源类型的对象。
    /// </summary>
    /// <param name="source">指定源类型的源对象</param>
    TDestination MapTo<TSource, TDestination>(TSource source);
}
