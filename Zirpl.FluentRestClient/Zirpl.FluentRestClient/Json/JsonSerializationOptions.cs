namespace Zirpl.FluentRestClient.Json
{
    [Flags]
    public enum JsonSerializationOptions
    {
        None = 0,
        Formatted = 1,
        IncludeWritablePropertiesOnly = 2,
        PreserveObjectReferences = 4,
        CamelCasing = 8
    }
}