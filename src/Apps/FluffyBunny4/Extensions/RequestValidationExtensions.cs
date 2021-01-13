using System;
using System.Collections.Generic;
using System.Text.Json;

namespace FluffyBunny4.Extensions
{
    static class RequestValidationExtensions
    {
        public static (bool, T) ValidateFormat<T>(this List<string> errorList, string name, string json)
        {
            T values = default;
            bool error = false;
            try
            {

                if (!string.IsNullOrWhiteSpace(json))
                {
                    JsonSerializerOptions options = new JsonSerializerOptions
                    {
                        IgnoreNullValues = true,
                        PropertyNameCaseInsensitive = true
                    };
                    values =  System.Text.Json.JsonSerializer.Deserialize<T>(json, options);
                }

            }
            catch (Exception)
            {
                error = true;
                errorList.Add($"{name} is malformed!");
            }

            return (error, values);
        }
    }
}
