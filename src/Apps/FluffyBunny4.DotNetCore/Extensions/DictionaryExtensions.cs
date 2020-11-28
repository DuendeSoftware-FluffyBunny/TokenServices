using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace System.Collections.Generic
{
    public static class DictionaryExtensions
    {
        public static void AddOptional(this IDictionary<string, string> parameters, string key, string value)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            if (key.IsMissing()) throw new ArgumentNullException(nameof(key));

            if (value.IsPresent())
            {
                if (parameters.ContainsKey(key))
                {
                    throw new InvalidOperationException($"Duplicate parameter: {key}");
                }
                else
                {
                    parameters.Add(key, value);
                }
            }
        }

        public static void AddRequired(this IDictionary<string, string> parameters, string key, string value, bool allowEmpty = false)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            if (key.IsMissing()) throw new ArgumentNullException(nameof(key));

            if (parameters.ContainsKey(key))
            {
                throw new InvalidOperationException($"Duplicate parameter: {key}");
            }
            else if (value.IsPresent() || allowEmpty)
            {
                parameters.Add(key, value);
            }
            else
            {
                throw new ArgumentException($"Parameter is required", key);
            }
        }
    }
}
