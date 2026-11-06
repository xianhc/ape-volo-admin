using Ape.Volo.Core.ConfigOptions;

namespace Ape.Volo.Core.IdGeneration;

internal class SnowWorkerVm
{
    /// <summary>
    /// 基础时间
    /// </summary>
    protected readonly DateTime BaseTime = new(2024, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc);

    /// <summary>
    /// 机器码
    /// </summary>
    protected readonly ushort WorkerId = 0;

    private readonly uint _dataCenterId;
    internal const byte DataCenterIdBitLength = 5;

    /// <summary>
    /// 机器码位长
    /// </summary>
    internal const byte WorkerIdBitLength = 6;

    /// <summary>
    /// 自增序列数位长
    /// </summary>
    protected const byte SeqBitLength = 6;

    /// <summary>
    /// 最大序列数（含）
    /// </summary>
    protected readonly int MaxSeqNumber = (1 << SeqBitLength) - 1;

    /// <summary>
    /// 最小序列数（含）
    /// </summary>
    protected readonly ushort MinSeqNumber = 5;

    /// <summary>
    /// 最大漂移次数（含）
    /// </summary>
    protected int TopOverCostCount = 2000;

    protected byte TimestampShift = 0;
    protected readonly object SyncLock = new();

    protected ushort CurrentSeqNumber = 0;
    protected long LastTimeTick = 0; // -1L
    protected long TurnBackTimeTick = 0; // -1L;
    protected byte TurnBackIndex = 0;
    protected bool IsOverCost = false;
    protected int OverCostCountInOneTerm = 0;

    //private static long _StartTimeTick = 0;
    //private static long _BaseTimeTick = 0;


    /// <summary>
    /// 初始化 <see cref="SnowWorkerVm"/> 的新实例。
    /// </summary>
    /// <param name="options">options</param>

    public SnowWorkerVm(SnowflakeIdOptions options)
    {
        WorkerId = options.WorkerId;
        _dataCenterId = options.DataCenterId;
        TimestampShift = DataCenterIdBitLength + WorkerIdBitLength + SeqBitLength;
        CurrentSeqNumber = MinSeqNumber;

        //_BaseTimeTick = BaseTime.Ticks;
        //_StartTimeTick = (long)(DateTime.UtcNow.Subtract(BaseTime).TotalMilliseconds) - Environment.TickCount;
    }

    protected virtual long NextOverCostId()
    {
        long currentTimeTick = GetCurrentTimeTick();

        if (currentTimeTick > LastTimeTick)
        {
            LastTimeTick = currentTimeTick;
            CurrentSeqNumber = MinSeqNumber;
            IsOverCost = false;
            OverCostCountInOneTerm = 0;

            return CalcId(LastTimeTick);
        }

        if (OverCostCountInOneTerm >= TopOverCostCount)
        {
            // TODO: 在漂移终止，等待时间对齐时，如果发生时间回拨较长，则此处可能等待较长时间。可优化为：在漂移终止时增加时间回拨应对逻辑。（该情况发生概率低，暂不处理）

            LastTimeTick = GetNextTimeTick();
            CurrentSeqNumber = MinSeqNumber;
            IsOverCost = false;
            OverCostCountInOneTerm = 0;

            return CalcId(LastTimeTick);
        }

        if (CurrentSeqNumber > MaxSeqNumber)
        {
            LastTimeTick++;
            CurrentSeqNumber = MinSeqNumber;
            IsOverCost = true;
            OverCostCountInOneTerm++;

            return CalcId(LastTimeTick);
        }

        return CalcId(LastTimeTick);
    }

    protected virtual long NextNormalId()
    {
        long currentTimeTick = GetCurrentTimeTick();

        if (currentTimeTick < LastTimeTick)
        {
            if (TurnBackTimeTick < 1)
            {
                TurnBackTimeTick = LastTimeTick - 1;

                TurnBackIndex++;
                // 每毫秒序列数的前5位是预留位，0用于手工新值，1-4是时间回拨次序
                // 支持4次回拨次序（避免回拨重叠导致ID重复），可无限次回拨（次序循环使用）。
                if (TurnBackIndex > 4)
                {
                    TurnBackIndex = 1;
                }
            }

            //Thread.Sleep(1);
            return CalcTurnBackId(TurnBackTimeTick);
        }

        // 时间追平时，_TurnBackTimeTick清零
        if (TurnBackTimeTick > 0)
        {
            TurnBackTimeTick = 0;
        }

        if (currentTimeTick > LastTimeTick)
        {
            LastTimeTick = currentTimeTick;
            CurrentSeqNumber = MinSeqNumber;

            return CalcId(LastTimeTick);
        }

        if (CurrentSeqNumber > MaxSeqNumber)
        {
            OverCostCountInOneTerm = 1;
            LastTimeTick++;
            CurrentSeqNumber = MinSeqNumber;
            IsOverCost = true;

            return CalcId(LastTimeTick);
        }

        return CalcId(LastTimeTick);
    }

    protected virtual long CalcId(long useTimeTick)
    {
        var result = (useTimeTick << TimestampShift) +
                     ((long)_dataCenterId << (WorkerIdBitLength + SeqBitLength)) +
                     ((long)WorkerId << SeqBitLength) +
                     (uint)CurrentSeqNumber;

        CurrentSeqNumber++;
        return result;
    }

    protected virtual long CalcTurnBackId(long useTimeTick)
    {
        var result = (useTimeTick << TimestampShift) +
                     ((long)_dataCenterId << (WorkerIdBitLength + SeqBitLength)) +
                     ((long)WorkerId << SeqBitLength) + TurnBackIndex;

        TurnBackTimeTick--;
        return result;
    }

    protected virtual long GetCurrentTimeTick()
    {
        //return (long)(DateTime.UtcNow - BaseTime).Ticks;
        //return (long)(_StartTimeTick + Environment.TickCount);
        return (long)(DateTime.UtcNow - BaseTime).TotalMilliseconds;
    }

    protected virtual long GetNextTimeTick()
    {
        long tempTimeTicker = GetCurrentTimeTick();

        while (tempTimeTicker <= LastTimeTick)
        {
            //Thread.Sleep(1);
            SpinWait.SpinUntil(() => false, 1);
            tempTimeTicker = GetCurrentTimeTick();
        }

        return tempTimeTicker;
    }


    public virtual long NextId()
    {
        lock (SyncLock)
        {
            return IsOverCost ? NextOverCostId() : NextNormalId();
        }
    }
}
