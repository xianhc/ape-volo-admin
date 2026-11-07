using Ape.Volo.Core.Serilog;
using Ape.Volo.Web.Logging;
using Serilog;
using Xunit;

namespace Ape.Volo.Web.Tests;

[CollectionDefinition("ConsoleLogging", DisableParallelization = true)]
public class ConsoleLoggingCollection;

[Collection("ConsoleLogging")]
public class ConsoleLoggingTests
{
    [Theory]
    [InlineData("APE-VOLO")]
    [InlineData("CUSTOM")]
    [InlineData("{Literal}")]
    public void ConsolePrefixAppliesToApplicationAndSqlLogs(string prefix)
    {
        var original = Console.Out;
        using var output = new StringWriter();
        try
        {
            Console.SetOut(output);
            using var logger = new LoggerConfiguration().WriteToConsole(prefix).CreateLogger();
            logger.Information("Application message");
            logger.ForContext(LoggerProperty.RecordSqlLog, true)
                .ForContext(LoggerProperty.ToConsole, true)
                .Information("SQL message");
        }
        finally
        {
            Console.SetOut(original);
        }

        var lines = output.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(2, lines.Length);
        Assert.All(lines, line => Assert.StartsWith($"[{prefix}] ", line));
    }
}
