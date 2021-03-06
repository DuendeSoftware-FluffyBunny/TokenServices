﻿using System;
using System.Threading.Tasks;
using Duende.IdentityServer.Stores;

namespace FluffyBunny4.Stores
{
    public interface IPersistedGrantStoreEx : IPersistedGrantStore
    {
        Task CopyAsync(string sourceKey, string destinationKey);

        //  Task RemoveAllAsync(string subjectId, string clientId, DateTime? before);

        //Task RemoveAllAsync(string subjectId, string clientId, string type, DateTime? before);
    }
}
