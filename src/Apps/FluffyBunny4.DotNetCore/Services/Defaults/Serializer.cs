﻿using FluffyBunny4.DotNetCore.Services;
using System.Text.Json;

namespace FluffyBunny4.DotNetCore.Services
{
    public class Serializer : ISerializer
    {
        public T Deserialize<T>(string text) where T : class
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Deserialize<T>(text, options);
        }

        public string Serialize<T>(T obj, bool indent = false) where T : class
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = indent,
                IgnoreNullValues = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return JsonSerializer.Serialize(obj, options);
        }

    }
}
