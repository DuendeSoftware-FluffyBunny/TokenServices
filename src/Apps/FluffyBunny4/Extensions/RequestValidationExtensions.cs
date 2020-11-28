using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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
                    values =
                        JsonConvert.DeserializeObject<T>(json);
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
