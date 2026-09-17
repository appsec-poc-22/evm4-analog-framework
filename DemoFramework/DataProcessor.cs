using Newtonsoft.Json;

namespace DemoFramework;

/// <summary>
/// Stands in for Crossbones.Modules.*: shared framework code that wraps a
/// third-party library. Service code calls this; it never touches
/// Newtonsoft.Json directly.
/// </summary>
public static class DataProcessor
{
    /// <summary>
    /// Calls the vulnerable Newtonsoft.Json deserialization API.
    /// Reachability test: can a tool trace DemoService's controller through
    /// here and into JsonConvert?
    /// </summary>
    public static T? Parse<T>(string payload)
    {
        // TypeNameHandling.All makes the deserialization path genuinely unsafe,
        // not merely present.
        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };
        return JsonConvert.DeserializeObject<T>(payload, settings);
    }

    public static string Serialize(object value) => JsonConvert.SerializeObject(value);
}
