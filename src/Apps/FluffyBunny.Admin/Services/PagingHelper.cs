using System;
using System.Collections.Generic;
using FluffyBunny4.DotNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FluffyBunny.Admin.Services
{
    internal class PagingHelper : IPagingHelper
    {
        private IHttpContextAccessor _httpContextAccessor;

        public PagingHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public List<SelectListItem> GetPagingSizeOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem("4", "4"),
                new SelectListItem("8", "8"),
                new SelectListItem("16", "16"),
                new SelectListItem("32", "32"),
            };
        }

        public int ValidatePageSize(int? pageSize)
        {
            int validatedPageSize = 4;
            if (pageSize == null)
            {
                var sPageSize = _httpContextAccessor.HttpContext.GetStringCookie(Constants.Cookies.PagingSizeName);
                if (string.IsNullOrWhiteSpace(sPageSize))
                {
                    validatedPageSize = 4;
                }
                else
                {
                    validatedPageSize = Convert.ToInt32(sPageSize);
                    int divisor = validatedPageSize / 4;
                    validatedPageSize = divisor * 4;
                }
            }
            else
            {
                validatedPageSize = (int)pageSize;

            }

            if (validatedPageSize <= 4 || validatedPageSize > 32)
            {
                validatedPageSize = 4;
            }
            _httpContextAccessor.HttpContext.SetStringCookie(
                Constants.Cookies.PagingSizeName, validatedPageSize.ToString(), 60 * 24 * 30);

            return validatedPageSize;
        }
    }
}