using Serilog;

namespace Ape.Volo.Core.Serilog;

public class SerilogManager
{
    public static ILogger GetLogger(Type type)
    {
        return Log.ForContext("SourceContext", type.FullName);
    }
}
