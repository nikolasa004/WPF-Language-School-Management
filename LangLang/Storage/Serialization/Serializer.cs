using System.Collections.Generic;
using System.Text.Json;

namespace LangLang.Storage.Serialization
{
    class Serializer<T> where T : ISerializable, new()
    {
        public string ToJSON(List<T> objects)
        {
            var options = new JsonSerializerOptions{WriteIndented = true};
            return JsonSerializer.Serialize(objects,options);
        }
        public List<T> FromJSON(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new List<T>();

            return JsonSerializer.Deserialize<List<T>>(content) ?? new List<T>();
        }
    }
}