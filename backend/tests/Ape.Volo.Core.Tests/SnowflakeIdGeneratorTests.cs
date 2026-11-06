using Ape.Volo.Application.Contracts.Abstractions;
using Ape.Volo.Core.ConfigOptions;
using Ape.Volo.Core.IdGeneration;
using Xunit;

namespace Ape.Volo.Core.Tests;

public sealed class SnowflakeIdGeneratorTests
{
    [Fact]
    public void ConcurrentCallsProduceUniquePositiveIds()
    {
        IIdGenerator generator = new SnowflakeIdGenerator(new SnowflakeIdOptions());
        var ids = new long[20000];
        Parallel.For(0, ids.Length, index => ids[index] = generator.NewId());

        Assert.All(ids, id => Assert.True(id > 0));
        Assert.Equal(ids.Length, ids.Distinct().Count());
    }

    [Fact]
    public void SequentialCallsProduceIncreasingIdsAcrossSequenceRollover()
    {
        IIdGenerator generator = new SnowflakeIdGenerator(new SnowflakeIdOptions());
        var previous = generator.NewId();
        for (var index = 0; index < 10000; index++)
        {
            var current = generator.NewId();
            Assert.True(current > previous);
            previous = current;
        }
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(63, 31)]
    public void IdsEncodeBothWorkerAndDataCenter(int workerId, int dataCenterId)
    {
        IIdGenerator generator = new SnowflakeIdGenerator(new SnowflakeIdOptions
        {
            WorkerId = (ushort)workerId,
            DataCenterId = (uint)dataCenterId
        });

        for (var index = 0; index < 1000; index++)
        {
            var id = generator.NewId();
            Assert.Equal(workerId, (id >> 6) & 63);
            Assert.Equal(dataCenterId, (id >> 12) & 31);
        }
    }

    [Theory]
    [InlineData(64, 0, "WorkerId")]
    [InlineData(1, 32, "DataCenterId")]
    public void InvalidNodeIdsAreRejected(int workerId, int dataCenterId, string parameterName)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new SnowflakeIdGenerator(new SnowflakeIdOptions
        {
            WorkerId = (ushort)workerId,
            DataCenterId = (uint)dataCenterId
        }));
        Assert.Equal(parameterName, exception.ParamName);
    }
}
