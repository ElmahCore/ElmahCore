using System.Text.Json;
using System.Text.Json.Serialization;

namespace ElmahCore.Mvc.Handlers;

internal static class DefaultJsonSerializerOptions
{
    public readonly static JsonSerializerOptions ApiSerializerOptions = new()
    {
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        MaxDepth = 0
    };

    public readonly static JsonSerializerOptions IgnoreDefault = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };
}
