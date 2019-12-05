using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Claims;
using System.Web;

/// <summary>
/// Summary description for Msdal
/// </summary>
public class Msdal
{
    public class MSALUserTokenMemoryCache
    {
        private readonly string appId;
        private readonly MemoryCache memoryCache = MemoryCache.Default;
        private readonly DateTimeOffset cacheDuration = DateTimeOffset.Now.AddHours(12);

        public MSALUserTokenMemoryCache(string clientId, ITokenCache userTokenCache)
        {
            this.appId = clientId;

            userTokenCache.SetBeforeAccess(this.UserTokenCacheBeforeAccessNotification);
            userTokenCache.SetAfterAccess(this.UserTokenCacheAfterAccessNotification);
        }

        public void LoadUserTokenCacheFromMemory(TokenCacheNotificationArgs args)
        {
            // Ideally, methods that load and persist should be thread safe. MemoryCache.Get() is thread safe.
            byte[] tokenCacheBytes = (byte[])this.memoryCache.Get(this.GetSignedInUsersCacheKey());
            if (tokenCacheBytes != null)
            {
                args.TokenCache.DeserializeMsalV3(tokenCacheBytes);
            }
        }

        public void PersistUserTokenCache(TokenCacheNotificationArgs args)
        {
            // Ideally, methods that load and persist should be thread safe.MemoryCache.Get() is thread safe.
            this.memoryCache.Set(this.GetSignedInUsersCacheKey(), args.TokenCache.SerializeMsalV3(), this.cacheDuration);
        }

        public void Clear()
        {
            this.memoryCache.Remove(this.GetSignedInUsersCacheKey());
        }

        private void UserTokenCacheAfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                this.PersistUserTokenCache(args);
            }
        }

        private void UserTokenCacheBeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            this.LoadUserTokenCacheFromMemory(args);
        }

        public string GetSignedInUsersCacheKey()
        {
            string objectIdClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";
            string signedInUsersId = string.Empty;

            if (ClaimsPrincipal.Current != null)
            {
                signedInUsersId = ClaimsPrincipal.Current.FindFirst(objectIdClaimType)?.Value;
            }
            return $"{this.appId}_UserTokenCache_{signedInUsersId}";
        }
    }
}