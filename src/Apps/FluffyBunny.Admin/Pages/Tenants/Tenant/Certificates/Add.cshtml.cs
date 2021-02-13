using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using FluffyBunny.Admin.Model;
using FluffyBunny.Admin.Services;
using FluffyBunny.CryptoServices;
using FluffyBunny.EntityFramework.Entities;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using FluffyBunny4.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluffyBunny.Admin.Pages.Tenants.Tenant.Certificates
{
    public class AddModel : PageModel
    {
        private ICryptoServices _cryptoServices;
        private CertificatesOptions _certificatesOptions;
        private IAdminServices _adminServices;
        private ISessionTenantAccessor _sessionTenantAccessor;
        private ILogger _logger;

        public AddModel(
            ICryptoServices cryptoServices,
            IOptions<CertificatesOptions> certificatesOptions,
            IAdminServices adminServices,
            ISessionTenantAccessor sessionTenantAccessor,
            ILogger<AddModel> logger)
        {
            _cryptoServices = cryptoServices;
            _certificatesOptions = certificatesOptions.Value;
            _adminServices = adminServices;
            _sessionTenantAccessor = sessionTenantAccessor;
            _logger = logger;
        }
        [BindProperty]
        public string TenantId { get; set; }
        public enum SigningAlgorithmType
        {
            RSA,
            ECDsa
        }

        public class InputModel
        {
            [Display(Name = "SigningAlgorithmType")]
            public SigningAlgorithmType SigningAlgorithmType { get; set; }
            public string[] SigningAlgorithmTypes = new[] { SigningAlgorithmType.RSA.ToString(), SigningAlgorithmType.ECDsa.ToString() };

            [Required]
            public DateTime NotBefore { get; set; }
            [Required]
            public DateTime Expiration { get; set; }
 
        }
        [BindProperty]
        public InputModel Input { get; set; }
        public async Task OnGetAsync()
        {
            var utcNow = DateTime.UtcNow;
            TenantId = _sessionTenantAccessor.TenantId;
            Input = new InputModel()
            {
                SigningAlgorithmType = SigningAlgorithmType.ECDsa,
                NotBefore = utcNow,
                Expiration = utcNow.AddMonths(9)
            };
        }
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                string pfx = "";
                switch (Input.SigningAlgorithmType)
                {
                    default:
                    case SigningAlgorithmType.ECDsa:
                        pfx = _cryptoServices.CreateECDsaCertificatePFX(_certificatesOptions.DnsName,
                            Input.NotBefore, Input.Expiration, _certificatesOptions.Password);
                        break;
                    case SigningAlgorithmType.RSA:
                        pfx = _cryptoServices.CreateRSACertificatePFX(_certificatesOptions.DnsName,
                            Input.NotBefore, Input.Expiration, _certificatesOptions.Password);
                        break;
                }

                await _adminServices.UpsertCertificateAsync(TenantId, new EntityFramework.Entities.Certificate()
                {
                    Expiration = Input.Expiration,
                    NotBefore = Input.NotBefore,
                    PFXBase64 = pfx,
                    SigningAlgorithm = Input.SigningAlgorithmType.ToString()
                });
                
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
            return RedirectToPage("./Index");
        }
    }
}
