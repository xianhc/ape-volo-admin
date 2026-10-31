using Ape.Volo.Common.Attributes;

namespace Ape.Volo.Core.ConfigOptions;

[OptionsSettings]
public class SwaggerOptions
{
    public bool Enabled { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}
