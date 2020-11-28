using Microsoft.AspNetCore.Http;
using System;
using System.Text.Json;

namespace FluffyBunny4.DotNetCore.Extensions
{
    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = false,
                IgnoreNullValues = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var obj = JsonSerializer.Serialize(value, options);

            session.SetString(key, obj);
        }

        public static T Get<T>(this ISession session, string key)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                PropertyNameCaseInsensitive = true
            };
            var value = session.GetString(key);
            return value == null ? default :
                JsonSerializer.Deserialize<T>(value, options);
        }
        public static string GetSessionId(this ISession session)
        {
            if (!session.IsAvailable)
            {
                session.SetString(Guid.NewGuid().ToString(), "ensure");
            }
            return session.Id;
        }
    }
}
