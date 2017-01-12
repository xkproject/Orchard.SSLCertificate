using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orchard.Environment.Shell;
using Orchard.Settings;
using Orchard.SSLCertificate.Services;
using Orchard.SSLCertificate.Settings;
using Orchard.SSLCertificate.ViewModels;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Orchard.SSLCertificate.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ShellSettings _shellSettings;
        private readonly ISSLCertificateService _sslCertificateService;

        public AdminController(
            IAuthorizationService authorizationService,
            ISSLCertificateService sslCertificateService,
            ShellSettings shellSettings
            )
        {
            _authorizationService = authorizationService;
            _sslCertificateService = sslCertificateService;
            _shellSettings = shellSettings;
        }
        
        public async Task<IActionResult> Edit(string returnUrl)
        {
            if (_shellSettings.Name!= "Default" 
                || !await _authorizationService.AuthorizeAsync(User, Permissions.ManageSettings))
                return Unauthorized();
            var sslCertificateSettings = _sslCertificateService.GetSSLCertificateSettings();
            
            var model = new SSLCertificateSettingsViewModel();
            model.CertificateStoreLocation = sslCertificateSettings.StoreLocation;
            model.CertificateStoreName = sslCertificateSettings.StoreName;
            model.CertificateThumbPrint = sslCertificateSettings.ThumbPrint;
            model.AvailableCertificates = _sslCertificateService.GetAvailableCertificates(onlyCertsWithPrivateKey: false);

            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SSLCertificateSettingsViewModel model, string returnUrl)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSettings))
                return Unauthorized();

            var settings = new SSLCertificateSettings();
            settings.StoreLocation = model.CertificateStoreLocation;
            settings.StoreName = model.CertificateStoreName;
            settings.ThumbPrint = model.CertificateThumbPrint;
            
            _sslCertificateService.IsValidSSLCertificateSettings(settings, ModelState);

            if (ModelState.IsValid)
            {
                _sslCertificateService.UpdateSSLCertificateSettings(settings);
                return RedirectToLocal(returnUrl);
            }

            model.AvailableCertificates = _sslCertificateService.GetAvailableCertificates(onlyCertsWithPrivateKey: false);
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return Redirect("~/");
            }
        }

    }
}
