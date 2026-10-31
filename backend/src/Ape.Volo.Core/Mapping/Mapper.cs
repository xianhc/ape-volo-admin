using Ape.Volo.Application.Contracts.Abstractions;
using Mapster;

namespace Ape.Volo.Core.Mapping;

/// <summary>
/// 基于宿主独立配置的 Mapster 适配器。
/// </summary>
/// <param name="config">Mapster 全局配置</param>
public sealed class Mapper(TypeAdapterConfig config) : IMapper
{
    /// <inheritdoc />
    public TDestination MapTo<TDestination>(object source) => source.Adapt<TDestination>(config);
    /// <inheritdoc />
    public TDestination MapTo<TSource, TDestination>(TSource source) => source.Adapt<TSource, TDestination>(config);
}
