using System;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.EntityFramework.Interfaces;
using Duende.IdentityServer.EntityFramework.Stores;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.Stores.Serialization;
using FluffyBunny.EntityFramework.Context;
using FluffyBunny4.Models;
using FluffyBunny4.Services;
using IdentityModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.IdentityServer.EntityFramework.Storage.Stores
{
    /// <summary>
    /// Implementation of IDeviceFlowStore thats uses EF.
    /// </summary>
    /// <seealso cref="IDeviceFlowStore" />
    public class EntityFrameworkDeviceFlowStoreExtra : IDeviceFlowStore
    {
       

        /// <summary>
        ///  The serializer.
        /// </summary>
        protected readonly IPersistentGrantSerializer Serializer;
        private readonly ICoreMapperAccessor _coreMapperAccessor;

        /// <summary>
        /// The logger.
        /// </summary>
        protected readonly ILogger Logger;
        private IScopedTenantRequestContext _scopedTenantRequestContext;
        private readonly ITenantAwareConfigurationDbContextAccessor _tenantAwareConfigurationDbContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceFlowStore"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="serializer">The serializer</param>
        /// <param name="logger">The logger.</param>
        public EntityFrameworkDeviceFlowStoreExtra(
            IScopedTenantRequestContext scopedTenantRequestContext,
            ITenantAwareConfigurationDbContextAccessor tenantAwareConfigurationDbContextAccessor,
            IPersistentGrantSerializer serializer,
            ICoreMapperAccessor coreMapperAccessor,
            ILogger<EntityFrameworkDeviceFlowStoreExtra> logger)
        {
            _scopedTenantRequestContext = scopedTenantRequestContext;
            _tenantAwareConfigurationDbContextAccessor = tenantAwareConfigurationDbContextAccessor;
            var tenant = _scopedTenantRequestContext.TenantId;
            Context = _tenantAwareConfigurationDbContextAccessor.GetTenantAwareConfigurationDbContext(tenant);

            Serializer = serializer;
            _coreMapperAccessor = coreMapperAccessor;
            Logger = logger;
        }

        public ITenantAwareConfigurationDbContext Context { get; set; }

        /// <summary>
        /// Stores the device authorization request.
        /// </summary>
        /// <param name="deviceCode">The device code.</param>
        /// <param name="userCode">The user code.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public virtual async Task StoreDeviceAuthorizationAsync(string deviceCode, string userCode, DeviceCode data)
        {
            var dataExtra = _coreMapperAccessor.Mapper.Map<DeviceCodeExtra>(data);

        
            Context.DeviceFlowCodes.Add(ToEntity(dataExtra, deviceCode, userCode));

            await Context.SaveChangesAsync();
        }

        /// <summary>
        /// Finds device authorization by user code.
        /// </summary>
        /// <param name="userCode">The user code.</param>
        /// <returns></returns>
        public virtual async Task<DeviceCode> FindByUserCodeAsync(string userCode)
        {
            var deviceFlowCodes = (await Context.DeviceFlowCodes.AsNoTracking().Where(x => x.UserCode == userCode).ToArrayAsync())
                .SingleOrDefault(x => x.UserCode == userCode);
            var model = ToModel(deviceFlowCodes?.Data);

            Logger.LogDebug("{userCode} found in database: {userCodeFound}", userCode, model != null);

            return model;
        }

        /// <summary>
        /// Finds device authorization by device code.
        /// </summary>
        /// <param name="deviceCode">The device code.</param>
        /// <returns></returns>
        public virtual async Task<DeviceCode> FindByDeviceCodeAsync(string deviceCode)
        {
            var deviceFlowCodes = (await Context.DeviceFlowCodes.AsNoTracking().Where(x => x.DeviceCode == deviceCode).ToArrayAsync())
                .SingleOrDefault(x => x.DeviceCode == deviceCode);
            var model = ToModel(deviceFlowCodes?.Data);

            Logger.LogDebug("{deviceCode} found in database: {deviceCodeFound}", deviceCode, model != null);

            return model;
        }

        /// <summary>
        /// Updates device authorization, searching by user code.
        /// </summary>
        /// <param name="userCode">The user code.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public virtual async Task UpdateByUserCodeAsync(string userCode, DeviceCode data)
        {
            var dataExtra = data as DeviceCodeExtra;

            var existing = (await Context.DeviceFlowCodes.Where(x => x.UserCode == userCode).ToArrayAsync())
                .SingleOrDefault(x => x.UserCode == userCode);
            if (existing == null)
            {
                Logger.LogError("{userCode} not found in database", userCode);
                throw new InvalidOperationException("Could not update device code");
            }
           
            var entity = ToEntity(dataExtra, existing.DeviceCode, userCode);
            Logger.LogDebug("{userCode} found in database", userCode);

            existing.SubjectId = data.Subject?.FindFirst(JwtClaimTypes.Subject).Value;
            existing.Data = entity.Data;

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Logger.LogWarning("exception updating {userCode} user code in database: {error}", userCode, ex.Message);
            }
        }

        /// <summary>
        /// Removes the device authorization, searching by device code.
        /// </summary>
        /// <param name="deviceCode">The device code.</param>
        /// <returns></returns>
        public virtual async Task RemoveByDeviceCodeAsync(string deviceCode)
        {
            var deviceFlowCodes = (await Context.DeviceFlowCodes.Where(x => x.DeviceCode == deviceCode).ToArrayAsync())
                .SingleOrDefault(x => x.DeviceCode == deviceCode);

            if (deviceFlowCodes != null)
            {
                Logger.LogDebug("removing {deviceCode} device code from database", deviceCode);

                Context.DeviceFlowCodes.Remove(deviceFlowCodes);

                try
                {
                    await Context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    Logger.LogInformation("exception removing {deviceCode} device code from database: {error}", deviceCode, ex.Message);
                }
            }
            else
            {
                Logger.LogDebug("no {deviceCode} device code found in database", deviceCode);
            }
        }

        /// <summary>
        /// Converts a model to an entity.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="deviceCode"></param>
        /// <param name="userCode"></param>
        /// <returns></returns>
        protected DeviceFlowCodes ToEntity(DeviceCodeExtra model, string deviceCode, string userCode)
        {
            if (model == null || deviceCode == null || userCode == null) return null;

            return new DeviceFlowCodes
            {
                DeviceCode = deviceCode,
                UserCode = userCode,
                ClientId = model.ClientId,
                SubjectId = model.Subject?.FindFirst(JwtClaimTypes.Subject).Value,
                CreationTime = model.CreationTime,
                Expiration = model.CreationTime.AddSeconds(model.Lifetime),
                Data = Serializer.Serialize(model)
            };
        }

        /// <summary>
        /// Converts a serialized DeviceCode to a model.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected DeviceCodeExtra ToModel(string entity)
        {
            if (entity == null) return null;

            return Serializer.Deserialize<DeviceCodeExtra>(entity);
        }
    }
}