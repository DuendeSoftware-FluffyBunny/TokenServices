using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Duende.IdentityServer.Extensions;

namespace FluffyBunny4.Services
{
    public class ScopedHttpContextRequestForm : IScopedHttpContextRequestForm
    {
        private IHttpContextAccessor _httpContextAccessor;

        public ScopedHttpContextRequestForm(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        NameValueCollection _form;
        public async Task<NameValueCollection> GetFormCollectionAsync()
        {
            if (_form == null 
                && _httpContextAccessor.HttpContext.Request.Method == "POST"
                && _httpContextAccessor.HttpContext.Request.ContentType == "application/x-www-form-urlencoded")
            {
                _form = (await _httpContextAccessor.HttpContext.Request.ReadFormAsync()).AsNameValueCollection();
            }
            return _form;
        }

        public NameValueCollection GetFormCollection()
        {
            return _form;
        }
    }
}
