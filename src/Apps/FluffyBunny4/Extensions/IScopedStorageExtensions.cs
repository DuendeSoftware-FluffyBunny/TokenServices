using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluffyBunny4.DotNetCore.Services;

namespace FluffyBunny4.Extensions
{
    public static class IScopedStorageExtensions
    {
        public static T Get<T>(this IScopedStorage scopedStorage, string key) where T : class
        {
            object obj;
            if (scopedStorage.TryGetValue(key, out obj))
            {
                return obj as T;
            }

            return null;
        }
    }
}
