using System.IO;
using Newtonsoft.Json;

namespace FluffyBunny4.Extensions
{
    public static class StreamExtensions
    {
        private static readonly JsonSerializer Serializer = new JsonSerializer();
        public static T FromStream<T>(this Stream stream)
        {
            using (stream)
            {
                if (typeof(Stream).IsAssignableFrom(typeof(T)))
                {
                    return (T)(object)stream;
                }

                using (StreamReader sr = new StreamReader(stream))
                {
                    using (JsonTextReader jsonTextReader = new JsonTextReader(sr))
                    {
                        return Serializer.Deserialize<T>(jsonTextReader);
                    }
                }
            }
        }
    }
}
