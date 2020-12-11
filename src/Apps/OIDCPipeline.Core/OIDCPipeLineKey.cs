using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OIDCPipeline.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OIDCPipeline.Core
{
    public interface IOIDCPipeLineKey
    {
        void SetOIDCPipeLineKey(string value);
        string GetOIDCPipeLineKey();
    }
    public class OIDCPipeLineKey : IOIDCPipeLineKey
    {
        public const string KeyName = ".oidc.Nonce.Tracker";
        private IDataProtectionProvider _dataProtectionProvider;
        private IHttpContextAccessor _httpContextAccessor;
     

        public OIDCPipeLineKey(IDataProtectionProvider dataProtectionProvider,IHttpContextAccessor httpContextAccessor)
        {
            _dataProtectionProvider = dataProtectionProvider;
            _httpContextAccessor = httpContextAccessor;
        }
        public string GetOIDCPipeLineKey()
        {
            var protector = _dataProtectionProvider.CreateProtector(KeyName);
            string protectedKey = _httpContextAccessor.HttpContext.GetStringCookie(KeyName);
            if (string.IsNullOrWhiteSpace(protectedKey))
            {
                return protectedKey;
            }
            var key = protector.Unprotect(protectedKey);
            return key;
        }

        public void SetOIDCPipeLineKey(string value)
        {
            var protector = _dataProtectionProvider.CreateProtector(KeyName);
            var protectedVal = protector.Protect(value);
            _httpContextAccessor.HttpContext.SetStringCookie(KeyName, protectedVal, 60);
        }
    }
   
}

