﻿using Newtonsoft.Json.Linq;
using System;

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
                    var obj = JToken.Parse(strInput);
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
