
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using static Orchard.SSLCertificate.Settings.SSLCertificateSettings;

namespace Orchard.SSLCertificate.ViewModels
{
    public class SSLCertificateSettingsViewModel
    {
        public StoreLocation? CertificateStoreLocation { get; set; }
        public StoreName? CertificateStoreName { get; set; }
        public string CertificateThumbPrint { get; set; }
        public IEnumerable<CertificateInfo> AvailableCertificates { get; set; }
    }
}
