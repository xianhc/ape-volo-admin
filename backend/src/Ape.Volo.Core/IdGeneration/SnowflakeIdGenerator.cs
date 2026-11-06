using Ape.Volo.Application.Contracts.Abstractions;
using Ape.Volo.Core.ConfigOptions;

namespace Ape.Volo.Core.IdGeneration;

/// <summary>
/// 雪花 ID 生成器，使用漂移算法处理序列耗尽和时钟回拨。
/// </summary>
public sealed class SnowflakeIdGenerator : IIdGenerator
{
    private readonly SnowWorkerVm _worker;

    /// <summary>
    /// 校验节点配置并创建进程内共享的生成器。
    /// </summary>
    /// <param name="options">节点配置</param>
    public SnowflakeIdGenerator(SnowflakeIdOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (options.WorkerId >= 1 << SnowWorkerVm.WorkerIdBitLength)
            throw new ArgumentOutOfRangeException(nameof(options.WorkerId), "SnowflakeId:WorkerId 必须在 0 至 63 之间。");
        if (options.DataCenterId >= 1 << SnowWorkerVm.DataCenterIdBitLength)
            throw new ArgumentOutOfRangeException(nameof(options.DataCenterId), "SnowflakeId:DataCenterId 必须在 0 至 31 之间。");

        _worker = new SnowWorkerVm(options);
        Thread.Sleep(TimeSpan.FromMilliseconds(500));
    }

    /// <inheritdoc />
    public long NewId() => _worker.NextId();
}
