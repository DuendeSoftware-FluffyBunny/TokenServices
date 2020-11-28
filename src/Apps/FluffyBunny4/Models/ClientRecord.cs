using Newtonsoft.Json;
using System.Collections.Generic;

namespace FluffyBunny4.Models
{
    public partial class ClientRecord
    {
        [JsonIgnore]
        public string ClientId { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }
        List<string> _secrets;
        [JsonProperty("secrets")]
        public List<string> Secrets
        {
            get
            {
                if (_secrets == null)
                {
                    _secrets = new List<string>();
                }
                return _secrets;
            }
            set
            {
                _secrets = value;
            }
        }
        List<ClaimHandle> _claimHandles;
        [JsonProperty("claims")]
        public List<ClaimHandle> ClaimHandles
        {
            get
            {
                if (_claimHandles == null)
                {
                    _claimHandles = new List<ClaimHandle>();
                }
                return _claimHandles;
            }
            set
            {
                _claimHandles = value;
            }
        }

        [JsonProperty("alwaysSendClientClaims")]
        public bool AlwaysSendClientClaims { get; set; }

        List<string> _allowedScopes;
        [JsonProperty("allowedScopes")]
        public List<string> AllowedScopes
        {
            get
            {
                if (_allowedScopes == null)
                {
                    _allowedScopes = new List<string>();
                }
                return _allowedScopes;
            }
            set
            {
                _allowedScopes = value;
            }
        }
        List<string> _allowedGrantTypes;
        [JsonProperty("AllowedGrantTypes")]
        public List<string> AllowedGrantTypes
        {
            get
            {
                if (_allowedGrantTypes == null)
                {
                    _allowedGrantTypes = new List<string>();
                }
                return _allowedGrantTypes;
            }
            set
            {
                _allowedGrantTypes = value;
            }
        }

        [JsonProperty("IdentityTokenLifetime")]
        public int IdentityTokenLifetime { get; set; }

        [JsonProperty("AccessTokenLifetime")]
        public int AccessTokenLifetime { get; set; }

        [JsonProperty("AuthorizationCodeLifetime")]
        public int AuthorizationCodeLifetime { get; set; }

        [JsonProperty("AbsoluteRefreshTokenLifetime")]
        public int AbsoluteRefreshTokenLifetime { get; set; }

        [JsonProperty("FrontChannelLogoutSessionRequired")]
        public bool FrontChannelLogoutSessionRequired { get; set; }

        [JsonProperty("FrontChannelLogoutUri")]
        public string FrontChannelLogoutUri { get; set; }

        [JsonProperty("SlidingRefreshTokenLifetime")]
        public int SlidingRefreshTokenLifetime { get; set; }
        List<string> _postLogoutRedirectUris;
        [JsonProperty("PostLogoutRedirectUris")]
        public List<string> PostLogoutRedirectUris
        {
            get
            {
                if (_postLogoutRedirectUris == null)
                {
                    _postLogoutRedirectUris = new List<string>();
                }
                return _postLogoutRedirectUris;
            }
            set
            {
                _postLogoutRedirectUris = value;
            }
        }

        List<string> _redirectUris;
        [JsonProperty("RedirectUris")]
        public List<string> RedirectUris
        {
            get
            {
                if (_redirectUris == null)
                {
                    _redirectUris = new List<string>();
                }
                return _redirectUris;
            }
            set
            {
                _redirectUris = value;
            }
        }

        [JsonProperty("RefreshTokenUsage")]
        public long RefreshTokenUsage { get; set; }

        [JsonProperty("AccessTokenType")]
        public long AccessTokenType { get; set; }

        [JsonProperty("AllowOfflineAccess")]
        public bool AllowOfflineAccess { get; set; }

        [JsonProperty("RequireClientSecret")]
        public bool RequireClientSecret { get; set; }

        [JsonProperty("RequireConsent")]
        public bool RequireConsent { get; set; }

        [JsonProperty("IncludeJwtId")]
        public bool IncludeJwtId { get; set; }

        private bool? _requireRefreshClientSecret;
        [JsonProperty("RequireRefreshClientSecret")]
        public bool RequireRefreshClientSecret
        {
            get
            {
                return _requireRefreshClientSecret == null ? false : (bool)_requireRefreshClientSecret;
            }
            set { _requireRefreshClientSecret = value; }
        }

        [JsonProperty("ClientClaimsPrefix")]
        public string ClientClaimsPrefix { get; set; }

        [JsonProperty("TenantId")]
        public string TenantId { get; internal set; }

        public bool? _includeClientId { get; set; }
        [JsonProperty("IncludeClientId")]
        public bool IncludeClientId
        {
            get
            {
                return _includeClientId == null ? false : (bool)_includeClientId;
            }
            set { _includeClientId = value; }
        }



        private bool? _allowGlobalSubjectRevocation;
        [JsonProperty("AllowGlobalSubjectRevocation")]
        public bool AllowGlobalSubjectRevocation
        {
            get
            {
                return _allowGlobalSubjectRevocation == null ? false : (bool)_allowGlobalSubjectRevocation;
            }
            set { _allowGlobalSubjectRevocation = value; }
        }

        private bool? _refreshTokenGraceEnabled;
        [JsonProperty("RefreshTokenGraceEnabled")]
        public bool RefreshTokenGraceEnabled
        {
            get
            {
                return _refreshTokenGraceEnabled == null ? false : (bool)_refreshTokenGraceEnabled;
            }
            set { _refreshTokenGraceEnabled = value; }
        }

        private int? _refreshTokenGraceTTL;
        [JsonProperty("RefreshTokenGraceTTL")]
        public int RefreshTokenGraceTTL
        {
            get
            {
                return _refreshTokenGraceTTL == null ? 0 : (int)_refreshTokenGraceTTL;
            }
            set { _refreshTokenGraceTTL = value; }
        }

        private int? _refreshTokenGraceMaxAttempts;
        [JsonProperty("RefreshTokenGraceMaxAttempts")]
        public int RefreshTokenGraceMaxAttempts
        {
            get
            {
                return _refreshTokenGraceMaxAttempts == null ? 0 : (int)_refreshTokenGraceMaxAttempts;
            }
            set { _refreshTokenGraceMaxAttempts = value; }
        }
    }
}
