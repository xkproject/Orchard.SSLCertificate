using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Orchard.SSLCertificate.Settings
{
    public class SSLCertificateSettings
    {
        public StoreLocation? StoreLocation { get; set; }
        public StoreName? StoreName { get; set; }
        public string ThumbPrint { get; set; }
    }
}
