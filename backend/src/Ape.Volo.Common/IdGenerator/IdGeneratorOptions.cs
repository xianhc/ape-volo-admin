

namespace Ape.Volo.Common.IdGenerator;

/// <summary>
/// Id参数配置
/// </summary>
public class IdGeneratorOptions
{
    /// <summary>
    /// 初始化 <see cref="IdGeneratorOptions"/> 的新实例。
    /// </summary>

    public IdGeneratorOptions()
    {
    }

    /// <summary>
    /// 初始化 <see cref="IdGeneratorOptions"/> 的新实例。
    /// </summary>
    /// <param name="workerId">workerId</param>

    public IdGeneratorOptions(ushort workerId)
    {
        WorkerId = workerId;
    }

    /// <summary>
    /// 基础时间（UTC格式）
    /// 不能超过当前系统时间
    /// </summary>
    public DateTime BaseTime { get; set; } = new DateTime(2024, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc);

    /// <summary>
    /// 机器码
    /// 必须由外部设定，最大值 2^WorkerIdBitLength-1
    /// </summary>
    public ushort WorkerId { get; set; } = 0;

    /// <summary>
    /// 机器码位长
    /// 默认值6，取值范围 [1, 15]（要求：序列数位长+机器码位长不超过22）
    /// </summary>
    public byte WorkerIdBitLength { get; set; } = 6; //10;

    /// <summary>
    /// 序列数位长
    /// 默认值6，取值范围 [3, 21]（要求：序列数位长+机器码位长不超过22）
    /// </summary>
    public byte SeqBitLength { get; set; } = 6; //10;

    /// <summary>
    /// 最大序列数（含）
    /// 设置范围 [MinSeqNumber, 2^SeqBitLength-1]，默认值0，表示最大序列数取最大值（2^SeqBitLength-1]）
    /// </summary>
    public int MaxSeqNumber { get; set; } = 0;

    /// <summary>
    /// 最小序列数（含）
    /// 默认值5，取值范围 [5, MaxSeqNumber]，每毫秒的前5个序列数对应编号0-4是保留位，其中1-4是时间回拨相应预留位，0是手工新值预留位
    /// </summary>
    public ushort MinSeqNumber { get; set; } = 5;

    /// <summary>
    /// 最大漂移次数（含），
    /// 默认2000，推荐范围500-10000（与计算能力有关）
    /// </summary>
    public int TopOverCostCount { get; set; } = 2000;

    /// <summary>
    /// 数据中心ID（默认0）
    /// </summary>
    public uint DataCenterId { get; set; } = 0;

    /// <summary>
    /// 数据中心ID长度（默认0）
    /// </summary>
    public byte DataCenterIdBitLength { get; set; } = 0;

    /// <summary>
    /// 时间戳类型（0-毫秒，1-秒），默认0
    /// </summary>
    public byte TimestampType { get; set; } = 0;

    /// <summary>
    /// 在使用漂移算法时启动的休眠时间，默认500毫秒
    /// </summary>
    public TimeSpan SleepTime { get; set; } = TimeSpan.FromMilliseconds(500);
}
