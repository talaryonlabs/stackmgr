using YamlDotNet.Serialization;

namespace Talaryon.StackManager.Serialization;

/// <summary>
/// Centralized YAML serialization utilities.
/// Provides thread-safe, cached serializer and deserializer instances.
/// </summary>
public static class YamlSerializer
{
    private static readonly ISerializer _serializer = new SerializerBuilder()
        .Build();

    private static readonly IDeserializer _deserializer = new DeserializerBuilder()
        .IgnoreUnmatchedProperties()
        .Build();

    /// <summary>
    /// Gets the shared serializer instance.
    /// </summary>
    public static ISerializer Serializer => _serializer;

    /// <summary>
    /// Gets the shared deserializer instance.
    /// </summary>
    public static IDeserializer Deserializer => _deserializer;

    /// <summary>
    /// Serializes an object to YAML string.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize</typeparam>
    /// <param name="value">The object to serialize</param>
    /// <returns>The YAML string representation</returns>
    public static string Serialize<T>(T value)
    {
        return _serializer.Serialize(value);
    }

    /// <summary>
    /// Deserializes a YAML string to an object.
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize</typeparam>
    /// <param name="yaml">The YAML string to deserialize</param>
    /// <returns>The deserialized object</returns>
    public static T Deserialize<T>(string yaml)
    {
        return _deserializer.Deserialize<T>(yaml);
    }

    /// <summary>
    /// Deserializes a YAML string from a text reader.
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize</typeparam>
    /// <param name="reader">The text reader containing YAML</param>
    /// <returns>The deserialized object</returns>
    public static T Deserialize<T>(TextReader reader)
    {
        return _deserializer.Deserialize<T>(reader);
    }
}
