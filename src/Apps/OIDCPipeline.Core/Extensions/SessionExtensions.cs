using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace OIDCPipeline.Core.Extensions
{
    internal static class SessionExtensions
    {
        private static readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
            IgnoreNullValues = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value, options));
        }

        public static T Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) :
                JsonSerializer.Deserialize<T>(value, options);
        }
    }
}
