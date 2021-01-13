 
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace OIDCPipeline.Core.Logging
{
    /// <summary>
    /// Helper to JSON serialize object data for logging.
    /// </summary>
    internal static class LogSerializer
    {
        private static readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
            IgnoreNullValues = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };


        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <param name="logObject">The object.</param>
        /// <returns></returns>
        public static string Serialize(object logObject)
        {
            return JsonSerializer.Serialize(logObject, options);
        }
    }
}
