using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc
{
    static public class StringMaskExtensions
    {
        public static string Mask(this string source, int start,  char maskCharacter)
        {
            if (start > source.Length - 1)
            {
                throw new ArgumentException("Start position is greater than string length");
            }

            var maskLength = source.Length - start;
            string mask = new string(maskCharacter, maskLength);
            string unMaskStart = source.Substring(0, start);

            return $"{unMaskStart}{mask}";
        }

     }
}
