using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orchard.SSLCertificate.Settings;
using Orchard.SSLCertificate.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Orchard.SSLCertificate.Services
{
    public interface ISSLCertificateService
    {
        SSLCertificateSettings GetSSLCertificateSettings();
        void UpdateSSLCertificateSettings(SSLCertificateSettings SSLCertificateSettings);
        bool IsValidSSLCertificateSettings(SSLCertificateSettings SSLCertificateSettings);
        bool IsValidSSLCertificateSettings(SSLCertificateSettings settings, ModelStateDictionary modelState);
        IEnumerable<CertificateInfo> GetAvailableCertificates(bool onlyCertsWithPrivateKey);
        CertificateInfo GetCertificateInfo(StoreLocation storeLocation, StoreName storeName, string certThumbPrint);
        X509Certificate2 GetCertificate(StoreLocation storeLocation, StoreName storeName, string certThumbPrint);
        X509Certificate2 GetCertificate(StoreLocation storeLocation, StoreName storeName, string certThumbPrint, string friendlyName, string certSubjectName, string dnsName, Nullable<DateTime> notAfter);
    }
}
