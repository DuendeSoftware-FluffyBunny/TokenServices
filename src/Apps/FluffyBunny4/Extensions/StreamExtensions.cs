using System.IO;
using System.Text.Json;

namespace FluffyBunny4.Extensions
{
    public static class StreamExtensions
    {
     
        public static T FromStream<T>(this Stream stream)
        {
            using (stream)
            {
                if (typeof(Stream).IsAssignableFrom(typeof(T)))
                {
                    return (T)(object)stream;
                }
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    IgnoreNullValues = true,
                    PropertyNameCaseInsensitive = true
                };
                
                using (StreamReader sr = new StreamReader(stream))
                {
                    return System.Text.Json.JsonSerializer.Deserialize<T>(sr.ReadToEnd(), options);
                }
            }
        }
    }
}
