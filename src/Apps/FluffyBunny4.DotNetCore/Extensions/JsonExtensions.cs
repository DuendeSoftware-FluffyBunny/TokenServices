 
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace FluffyBunny4.DotNetCore.Extensions
{
    public static class JsonExtensions
    {
        public static bool IsValidJson(this string strInput)
        {
            strInput = strInput.Trim();
            if (strInput.StartsWith("{") && strInput.EndsWith("}") || //For object
                strInput.StartsWith("[") && strInput.EndsWith("]")) //For array
            {
                try
                {
                    var obj = System.Text.Json.JsonDocument.Parse(strInput);
                    return true;
                }
                catch (Exception ex) //some other exception
                {
                    return false;
                }
            }
            return false;
        }
    }
}
