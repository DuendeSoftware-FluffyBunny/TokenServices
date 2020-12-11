using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.Mvc
{
    internal static class CookieExtensions
    {
        public static void SetStringCookie(this HttpResponse response, string key, string value, int? expireTime)
        {
            CookieOptions option = new CookieOptions()
            {
                IsEssential = true
            };

            if (expireTime.HasValue)
                option.Expires = DateTime.Now.AddMinutes(expireTime.Value);
            else
                option.Expires = DateTime.Now.AddMilliseconds(10);

            response.Cookies.Append(key, value, option);
        }
        public static void SetStringCookie(this HttpContext httpContext, string key, string value, int? expireTime)
        {
            httpContext.Response.SetStringCookie(key, value, expireTime);
        }
    
        /// <summary>  
        /// set the cookie  
        /// </summary>  
        /// <param name="key">key (unique indentifier)</param>  
        /// <param name="value">value to store in cookie object</param>  
        /// <param name="expireTime">expiration time</param>  
        public static void SetJsonCookie<T>(this HttpResponse response,string key, T value, int? expireTime)
        {
            response.SetStringCookie(key, JsonConvert.SerializeObject(value), expireTime);
        }
        public static void SetJsonCookie<T>(this HttpContext httpContext, string key, T value, int? expireTime)
        {
            httpContext.Response.SetJsonCookie<T>(key, value, expireTime);
        }
      
        public static string GetStringCookie(this HttpRequest request, string key) 
        {
            //read cookie from Request object  
            string cookieValueFromReq = request.Cookies[key];
            if (string.IsNullOrWhiteSpace(cookieValueFromReq))
            {
                return null;
            }
            return cookieValueFromReq;

        }
        public static T GetJsonCookie<T>(this HttpRequest request, string key) where T : class
        {
            //read cookie from Request object  
            string cookieValueFromReq = request.Cookies[key];
            if (string.IsNullOrWhiteSpace(cookieValueFromReq))
            {
                return null;
            }
            return JsonConvert.DeserializeObject<T>(cookieValueFromReq);

        }
      
        public static T GetJsonCookie<T>(this HttpContext httpContext, string key) where T : class
        {
            //read cookie from Request object  
            return httpContext.Request.GetJsonCookie<T>(key);
        }

      
        public static string GetStringCookie(this HttpContext httpContext, string key)  
        {
            //read cookie from Request object  
            return httpContext.Request.GetStringCookie(key);
        }
    }
}