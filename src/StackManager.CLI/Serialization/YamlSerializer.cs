using YamlDotNet.Serialization;

namespace Talaryon.StackManager.Serialization;

/// <summary>
/// Centralized YAML serialization utilities.
/// Provides thread-safe, cached serializer and deserializer instances.
/// </summary>
public static class YamlSerializer
{
    /// <summary>
    /// Gets the shared serializer instance.
    /// </summary>
    public static ISerializer Serializer => new SerializerBuilder()
        .Build();

    /// <summary>
    /// Gets the shared deserializer instance.
    /// </summary>
    public static IDeserializer Deserializer => new DeserializerBuilder()
        .IgnoreUnmatchedProperties()
        .Build();

    /// <summary>
    /// Serializes an object to YAML string.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize</typeparam>
    /// <param name="value">The object to serialize</param>
    /// <returns>The YAML string representation</returns>
    public static string Serialize<T>(T value)
    {
        return Serializer.Serialize(value);
    }

    /// <summary>
    /// Deserializes a YAML string to an object.
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize</typeparam>
    /// <param name="yaml">The YAML string to deserialize</param>
    /// <returns>The deserialized object</returns>
    public static T Deserialize<T>(string yaml)
    {
        return Deserializer.Deserialize<T>(yaml);
    }

    /// <summary>
    /// Deserializes a YAML string from a text reader.
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize</typeparam>
    /// <param name="reader">The text reader containing YAML</param>
    /// <returns>The deserialized object</returns>
    public static T Deserialize<T>(TextReader reader)
    {
        return Deserializer.Deserialize<T>(reader);
    }
}
