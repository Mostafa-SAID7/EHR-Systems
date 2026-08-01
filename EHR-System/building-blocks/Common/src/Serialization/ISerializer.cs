namespace EHRPlatform.Common.Serialization;

/// <summary>
/// Interface for serialization operations.
/// Single responsibility: Serialization contract.
/// </summary>
public interface ISerializer
{
    /// <summary>
    /// Serialize object to JSON string.
    /// </summary>
    string Serialize<T>(T obj);

    /// <summary>
    /// Deserialize JSON string to object.
    /// </summary>
    T? Deserialize<T>(string json);

    /// <summary>
    /// Serialize object to byte array.
    /// </summary>
    byte[] SerializeToBytes<T>(T obj);

    /// <summary>
    /// Deserialize byte array to object.
    /// </summary>
    T? DeserializeFromBytes<T>(byte[] data);
}
