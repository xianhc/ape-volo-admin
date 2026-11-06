using Ape.Volo.Common.Attributes;

namespace Ape.Volo.Core.ConfigOptions;

/// <summary>
/// 雪花 ID 节点配置；生成规则固定在实现内部。
/// </summary>
[OptionsSettings]
public sealed class SnowflakeIdOptions
{
    /// <summary>
    /// 机器码，范围 0 至 63，同一数据中心内各实例必须不同。
    /// </summary>
    public ushort WorkerId { get; set; } = 1;

    /// <summary>
    /// 数据中心 ID，范围 0 至 31。
    /// </summary>
    public uint DataCenterId { get; set; }
}
