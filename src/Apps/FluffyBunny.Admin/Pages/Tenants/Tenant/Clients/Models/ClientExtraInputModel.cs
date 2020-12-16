using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;

namespace FluffyBunny.Admin.Pages.Tenants.Tenant.Clients.Models
{
    public class ClientExtraInputModel
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public bool Readonly { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string ClientId { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string ClientName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string ProtocolType { get; set; }


        [Required]
        [DataType(DataType.Text)]
        public string TenantId { get; set; }
        [Required]
        public bool Enabled { get; set; }

        [Display(Name = "Refresh Token Usage")]
        public TokenUsage RefreshTokenUsage { get; set; }
        public string[] RefreshTokenUsages = new[] { TokenUsage.ReUse.ToString(), TokenUsage.OneTimeOnly.ToString() };

        [Display(Name = "Access Token Type")]
        public AccessTokenType AccessTokenType { get; set; }
        public string[] AccessTokenTypes = new[] { AccessTokenType.Jwt.ToString(), AccessTokenType.Reference.ToString() };

        [Display(Name = "Refresh Token Expiration")]
        public TokenExpiration RefreshTokenExpiration { get; set; }
        public string[] RefreshTokenExpirations = new[] { TokenExpiration.Absolute.ToString(), TokenExpiration.Sliding.ToString() };

/*
        [Required]
        [Display(Name = "Allowed Grant Types")]
        public List<BooleanTypeRecord> SelectedGrantTypes { get; set; }
*/
        [Required]
        public int RefreshTokenGraceMaxAttempts { get; set; }
        [Required]
        public int RefreshTokenGraceTTL { get; set; }

        [Required]
        public int AccessTokenLifetime { get; set; }

        [Required]
        public int IdentityTokenLifetime { get; set; }


        [Required]
        public int AbsoluteRefreshTokenLifetime { get; set; }
        [Required]
        public int SlidingRefreshTokenLifetime { get; set; }
        [Display(Name = "Allow Offline Access")]
        [Required]
        public bool AllowOfflineAccess { get; set; }
        public bool RefreshTokenGraceEnabled { get; set; }
        public bool IncludeClientId { get; set; }
        public bool IncludeJwtId { get; set; }
        public bool IncludeAmr { get; set; }
        public bool AllowGlobalSubjectRevocation { get; set; }
        public bool RequireRefreshClientSecret { get; set; }
        public bool RequireClientSecret { get; set; }


        [Required]
        public List<string> AllowedScopes { get; set; }
        public ICollection<ClientClaim> Claims { get; set; }
        public bool AlwaysSendClientClaims { get; set; }
    }
}
