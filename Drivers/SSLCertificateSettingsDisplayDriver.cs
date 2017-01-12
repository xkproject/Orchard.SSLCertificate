using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Settings.Services;
using Orchard.SSLCertificate.Services;
using Orchard.SSLCertificate.Settings;
using Orchard.SSLCertificate.ViewModels;
using System;
using System.Threading.Tasks;

namespace Orchard.SSLCertificate.Drivers
{
    public class SSLCertificateSettingsDisplayDriver : SiteSettingsSectionDisplayDriver<SSLCertificateSettings>
    {
        private readonly ISSLCertificateService _sslCertificateService;
        public SSLCertificateSettingsDisplayDriver(ISSLCertificateService sslCertificateService)
        {
            _sslCertificateService = sslCertificateService;
        }

        public override IDisplayResult Edit(SSLCertificateSettings settings, BuildEditorContext context)
        {
            return Shape<SSLCertificateSettingsViewModel>("SSLCertificateSettings_Edit", model =>
                {
                    model.CertificateStoreLocation = settings.StoreLocation;
                    model.CertificateStoreName = settings.StoreName;
                    model.CertificateThumbPrint = settings.ThumbPrint;
                    model.AvailableCertificates = _sslCertificateService.GetAvailableCertificates(onlyCertsWithPrivateKey:true);
                }).Location("Content:2").OnGroup("ssl");
        }

        public override async Task<IDisplayResult> UpdateAsync(SSLCertificateSettings settings, IUpdateModel updater, string groupId)
        {
            if (groupId == "ssl")
            {
                var model = new SSLCertificateSettingsViewModel();

                await updater.TryUpdateModelAsync(model, Prefix);
                settings.StoreLocation = model.CertificateStoreLocation;
                settings.StoreName = model.CertificateStoreName;
                settings.ThumbPrint = model.CertificateThumbPrint;

                _sslCertificateService.IsValidSSLCertificateSettings(settings, updater.ModelState);
            }

            return Edit(settings);
        }
    }
}
