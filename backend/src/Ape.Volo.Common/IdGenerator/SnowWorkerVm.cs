using Ape.Volo.Common.IdGenerator.Interfaces;

namespace Ape.Volo.Common.IdGenerator;

public class SnowWorkerVm : ISnowWorker
{
    /// <summary>
    /// 基础时间
    /// </summary>
    protected readonly DateTime BaseTime;

    /// <summary>
    /// 机器码
    /// </summary>
    protected readonly ushort WorkerId = 0;

    /// <summary>
    /// 机器码位长
    /// </summary>
    protected readonly byte WorkerIdBitLength = 0;

    /// <summary>
    /// 自增序列数位长
    /// </summary>
    protected readonly byte SeqBitLength = 0;

    /// <summary>
    /// 最大序列数（含）
    /// </summary>
    protected readonly int MaxSeqNumber = 0;

    /// <summary>
    /// 最小序列数（含）
    /// </summary>
    protected readonly ushort MinSeqNumber = 0;

    /// <summary>
    /// 最大漂移次数（含）
    /// </summary>
    protected int TopOverCostCount = 0;

    protected byte TimestampShift = 0;
    protected static object SyncLock = new object();

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

    public SnowWorkerVm(IdGeneratorOptions options)
    {
        // 1.BaseTime
        if (options.BaseTime != DateTime.MinValue)
        {
            BaseTime = options.BaseTime;
        }

        // 2.WorkerIdBitLength
        if (options.WorkerIdBitLength == 0)
        {
            WorkerIdBitLength = 6;
        }
        else
        {
            WorkerIdBitLength = options.WorkerIdBitLength;
        }

        // 3.WorkerId
        WorkerId = options.WorkerId;

        // 4.SeqBitLength
        if (options.SeqBitLength == 0)
        {
            SeqBitLength = 6;
        }
        else
        {
            SeqBitLength = options.SeqBitLength;
        }

        // 5.MaxSeqNumber
        if (options.MaxSeqNumber <= 0)
        {
            MaxSeqNumber = (1 << SeqBitLength) - 1;
        }
        else
        {
            MaxSeqNumber = options.MaxSeqNumber;
        }

        // 6.MinSeqNumber
        MinSeqNumber = options.MinSeqNumber;

        // 7.Others
        TopOverCostCount = options.TopOverCostCount;
        //if (TopOverCostCount == 0)
        //{
        //    TopOverCostCount = 2000;
        //}

        TimestampShift = (byte)(WorkerIdBitLength + SeqBitLength);
        CurrentSeqNumber = options.MinSeqNumber;

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
                     ((long)WorkerId << SeqBitLength) +
                     (uint)CurrentSeqNumber;

        CurrentSeqNumber++;
        return result;
    }

    protected virtual long CalcTurnBackId(long useTimeTick)
    {
        var result = (useTimeTick << TimestampShift) +
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
