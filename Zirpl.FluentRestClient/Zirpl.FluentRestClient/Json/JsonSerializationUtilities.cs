using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Zirpl.FluentRestClient.Json
{
    public static class JsonSerializationUtilities
    {
        public static string ToJson(this object data, JsonSerializationOptions options = JsonSerializationOptions.None)
        {
            var settings = new JsonSerializerSettings();
            settings.ContractResolver =
                GetContractResolver(options.HasFlag(JsonSerializationOptions.IncludeWritablePropertiesOnly),
                    options.HasFlag(JsonSerializationOptions.CamelCasing));

            if (options.HasFlag(JsonSerializationOptions.PreserveObjectReferences))
            {
                settings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
                settings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            }
            else
            {
                settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                settings.PreserveReferencesHandling = PreserveReferencesHandling.None;
            }
            return JsonConvert.SerializeObject(data, options.HasFlag(JsonSerializationOptions.Formatted) ? Formatting.Indented : Formatting.None, settings);
        }

        private static IContractResolver GetContractResolver(bool writablePropertiesOnly, bool camelCase)
        {
            if (writablePropertiesOnly)
            {
                return new IncludeWritablePropertiesOnlyResolver
                {
                    NamingStrategy = camelCase
                        ? (NamingStrategy) new CamelCaseNamingStrategy()
                        : (NamingStrategy) new DefaultNamingStrategy()
                };
            }
            else
            {
                return new DefaultContractResolver
                {
                    NamingStrategy = camelCase
                        ? (NamingStrategy)new CamelCaseNamingStrategy()
                        : (NamingStrategy)new DefaultNamingStrategy()
                };
            }
        }

        public static void ToJsonFile(this object data, string filePath, JsonSerializationOptions options = JsonSerializationOptions.None)
        {
            var text = data.ToJson(options);
            File.WriteAllText(filePath, text);
        }

        public static T FromJson<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static T FromJsonFile<T>(this string filePath)
        {
            var json = File.ReadAllText(filePath);
            return json.FromJson<T>();
        }

        private class IncludeWritablePropertiesOnlyResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                IList<JsonProperty> props = base.CreateProperties(type, memberSerialization);
                return props.Where(p => p.Writable).ToList();
            }
        }
    }
}