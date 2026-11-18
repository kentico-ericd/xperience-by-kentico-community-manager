namespace XperienceCommunity.Manager.Configuration;

/// <summary>
/// Represents a key from https://docs.kentico.com/documentation/developers-and-admins/configuration/reference-configuration-keys.
/// </summary>
public class ConfigurationKey(string keyName, string description, Type valueType, object? defaultValue = null)
{
    public string KeyName { get; private set; } = keyName;


    public string Description { get; private set; } = description;


    public Type ValueType { get; private set; } = valueType;


    public object? DefaultValue { get; private set; } = defaultValue;


    public object? ActualValue { get; set; }
}
