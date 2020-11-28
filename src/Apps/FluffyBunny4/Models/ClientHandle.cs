using System.Collections.Generic;
using Duende.IdentityServer.Models;

namespace FluffyBunny4.Models
{

    //
    // Summary:
    //     Models to serialize Client 
    public class ClientHandle 
    {
        public ClientHandle() { }

        //
        // Summary:
        //     Gets or sets a value indicating whether [allow offline access]. Defaults to false.
        public bool AllowOfflineAccess { get; set; }
        //
        // Summary:
        //     Lifetime of identity token in seconds (defaults to 300 seconds / 5 minutes)
        public int IdentityTokenLifetime { get; set; }
        //
        // Summary:
        //     Signing algorithm for identity token. If empty, will use the server default signing
        //     algorithm.
        public List<string> AllowedIdentityTokenSigningAlgorithms { get; set; }
        //
        // Summary:
        //     Lifetime of access token in seconds (defaults to 3600 seconds / 1 hour)
        public int AccessTokenLifetime { get; set; }
        //
        // Summary:
        //     Lifetime of authorization code in seconds (defaults to 300 seconds / 5 minutes)
        public int AuthorizationCodeLifetime { get; set; }
        //
        // Summary:
        //     Maximum lifetime of a refresh token in seconds. Defaults to 2592000 seconds /
        //     30 days
        public int AbsoluteRefreshTokenLifetime { get; set; }
        //
        // Summary:
        //     Sliding lifetime of a refresh token in seconds. Defaults to 1296000 seconds /
        //     15 days
        public int SlidingRefreshTokenLifetime { get; set; }
        //
        // Summary:
        //     Lifetime of a user consent in seconds. Defaults to null (no expiration)
        public int? ConsentLifetime { get; set; }
        //
        // Summary:
        //     ReUse: the refresh token handle will stay the same when refreshing tokens OneTime:
        //     the refresh token handle will be updated when refreshing tokens
        public TokenUsage RefreshTokenUsage { get; set; }
        //
        // Summary:
        //     Gets or sets a value indicating whether the access token (and its claims) should
        //     be updated on a refresh token request. Defaults to false.
        //
        // Value:
        //     true if the token should be updated; otherwise, false.
        public bool UpdateAccessTokenClaimsOnRefresh { get; set; }
        //
        // Summary:
        //     Absolute: the refresh token will expire on a fixed point in time (specified by
        //     the AbsoluteRefreshTokenLifetime) Sliding: when refreshing the token, the lifetime
        //     of the refresh token will be renewed (by the amount specified in SlidingRefreshTokenLifetime).
        //     The lifetime will not exceed AbsoluteRefreshTokenLifetime.
        public TokenExpiration RefreshTokenExpiration { get; set; }
        //
        // Summary:
        //     Specifies whether the access token is a reference token or a self contained JWT
        //     token (defaults to Jwt).
        public AccessTokenType AccessTokenType { get; set; }
        //
        // Summary:
        //     Gets or sets a value indicating whether the local login is allowed for this client.
        //     Defaults to true.
        //
        // Value:
        //     true if local logins are enabled; otherwise, false.
        public bool EnableLocalLogin { get; set; }
        //
        // Summary:
        //     Specifies which external IdPs can be used with this client (if list is empty
        //     all IdPs are allowed). Defaults to empty.
        public List<string> IdentityProviderRestrictions { get; set; }
        //
        // Summary:
        //     Gets or sets a value indicating whether JWT access tokens should include an identifier.
        //     Defaults to true.
        //
        // Value:
        //     true to add an id; otherwise, false.
        public bool IncludeJwtId { get; set; }
        //
        // Summary:
        //     Allows settings claims for the client (will be included in the access token).
        //
        // Value:
        //     The claims.
        public List<ClientClaim> Claims { get; set; }
        //
        // Summary:
        //     Gets or sets a value indicating whether client claims should be always included
        //     in the access tokens - or only for client credentials flow. Defaults to false
        //
        // Value:
        //     true if claims should always be sent; otherwise, false.
        public bool AlwaysSendClientClaims { get; set; }
        //
        // Summary:
        //     Gets or sets a value to prefix it on client claim types. Defaults to client_.
        //
        // Value:
        //     Any non empty string if claims should be prefixed with the value; otherwise,
        //     null.
        public string ClientClaimsPrefix { get; set; }
        //
        // Summary:
        //     Gets or sets a salt value used in pair-wise subjectId generation for users of
        //     this client.
        public string PairWiseSubjectSalt { get; set; }
        //
        // Summary:
        //     The maximum duration (in seconds) since the last time the user authenticated.
        public int? UserSsoLifetime { get; set; }
        //
        // Summary:
        //     Gets or sets the type of the device flow user code.
        //
        // Value:
        //     The type of the device flow user code.
        public string UserCodeType { get; set; }
        //
        // Summary:
        //     Gets or sets the device code lifetime.
        //
        // Value:
        //     The device code lifetime.
        public int DeviceCodeLifetime { get; set; }
        //
        // Summary:
        //     When requesting both an id token and access token, should the user claims always
        //     be added to the id token instead of requiring the client to use the userinfo
        //     endpoint. Defaults to false.
        public bool AlwaysIncludeUserClaimsInIdToken { get; set; }
        //
        // Summary:
        //     Specifies the api scopes that the client is allowed to request. If empty, the
        //     client can't access any scope
        public List<string> AllowedScopes { get; set; }
        //
        // Summary:
        //     Gets or sets the custom properties for the client.
        //
        // Value:
        //     The properties.
        public Dictionary<string, string> Properties { get; set; }
        //
        // Summary:
        //     Specifies is the user's session id should be sent to the BackChannelLogoutUri.
        //     Defaults to true.
        public bool BackChannelLogoutSessionRequired { get; set; }
        //
        // Summary:
        //     Specifies if client is enabled (defaults to true)
        public bool Enabled { get; set; }
        //
        // Summary:
        //     Unique ID of the client
        public string ClientId { get; set; }
        //
        // Summary:
        //     Gets or sets the protocol type.
        //
        // Value:
        //     The protocol type.
        public string ProtocolType { get; set; }
        //
        // Summary:
        //     Client secrets - only relevant for flows that require a secret
        public List<Secret> ClientSecrets { get; set; }
        //
        // Summary:
        //     If set to false, no client secret is needed to request tokens at the token endpoint
        //     (defaults to true)
        public bool RequireClientSecret { get; set; }
        //
        // Summary:
        //     Client display name (used for logging and consent screen)
        public string ClientName { get; set; }
        //
        // Summary:
        //     Description of the client.
        public string Description { get; set; }
        //
        // Summary:
        //     URI to further information about client (used on consent screen)
        public string ClientUri { get; set; }
        //
        // Summary:
        //     URI to client logo (used on consent screen)
        public string LogoUri { get; set; }
        //
        // Summary:
        //     Specifies whether a consent screen is required (defaults to false)
        public bool RequireConsent { get; set; }
        //
        // Summary:
        //     Specifies whether user can choose to store consent decisions (defaults to true)
        public bool AllowRememberConsent { get; set; }
        //
        // Summary:
        //     Specifies the allowed grant types (legal combinations of AuthorizationCode, Implicit,
        //     Hybrid, ResourceOwner, ClientCredentials).
        public List<string> AllowedGrantTypes { get; set; }
        //
        // Summary:
        //     Specifies whether a proof key is required for authorization code based token
        //     requests (defaults to true).
        public bool RequirePkce { get; set; }
        //
        // Summary:
        //     Specifies whether a proof key can be sent using plain method (not recommended
        //     and defaults to false.)
        public bool AllowPlainTextPkce { get; set; }
        //
        // Summary:
        //     Specifies whether the client must use a request object on authorize requests
        //     (defaults to false.)
        public bool RequireRequestObject { get; set; }
        //
        // Summary:
        //     Controls whether access tokens are transmitted via the browser for this client
        //     (defaults to false). This can prevent accidental leakage of access tokens when
        //     multiple response types are allowed.
        //
        // Value:
        //     true if access tokens can be transmitted via the browser; otherwise, false.
        public bool AllowAccessTokensViaBrowser { get; set; }
        //
        // Summary:
        //     Specifies allowed URIs to return tokens or authorization codes to
        public List<string> RedirectUris { get; set; }
        //
        // Summary:
        //     Specifies allowed URIs to redirect to after logout
        public List<string> PostLogoutRedirectUris { get; set; }
        //
        // Summary:
        //     Specifies logout URI at client for HTTP front-channel based logout.
        public string FrontChannelLogoutUri { get; set; }
        //
        // Summary:
        //     Specifies is the user's session id should be sent to the FrontChannelLogoutUri.
        //     Defaults to true.
        public bool FrontChannelLogoutSessionRequired { get; set; }
        //
        // Summary:
        //     Specifies logout URI at client for HTTP back-channel based logout.
        public string BackChannelLogoutUri { get; set; }
        //
        // Summary:
        //     Gets or sets the allowed CORS origins for JavaScript clients.
        //
        // Value:
        //     The allowed CORS origins.
        public List<string> AllowedCorsOrigins { get; set; }
    }
}
