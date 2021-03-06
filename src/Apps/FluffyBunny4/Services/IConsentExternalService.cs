﻿using FluffyBunny4.Models;
using FluffyBunny4.Models.Client;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FluffyBunny4.Services
{
    public interface IConsentExternalService
    {
        Task<ConsentAuthorizeResponseContainer<T>> PostAuthorizationRequestAsync<T>(ConsentDiscoveryDocumentResponse discovery, ConsentAuthorizeRequest requestObject,T context) where T:class;
    }
}
