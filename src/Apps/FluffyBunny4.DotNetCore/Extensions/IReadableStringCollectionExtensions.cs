using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace FluffyBunny4.DotNetCore.Extensions
{
    public static class IReadableStringCollectionExtensions
    {

        [DebuggerStepThrough]
        public static SimpleNameValueCollection AsNameValueCollection(this IEnumerable<KeyValuePair<string, StringValues>> collection)
        {
            var nv = new SimpleNameValueCollection();

            foreach (var field in collection)
            {
                nv.Add(field.Key, field.Value.First());
            }

            return nv;
        }

        [DebuggerStepThrough]
        public static SimpleNameValueCollection AsNameValueCollection(this IDictionary<string, StringValues> collection)
        {
            var nv = new SimpleNameValueCollection();

            foreach (var field in collection)
            {
                nv.Add(field.Key, field.Value.First());
            }

            return nv;
        }
    }
}
