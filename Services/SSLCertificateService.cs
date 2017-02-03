using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Orchard.SSLCertificate.Settings;
using Orchard.SSLCertificate.ViewModels;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Orchard.SSLCertificate.Services
{
    public class SSLCertificateService : ISSLCertificateService
    {
        private readonly IStringLocalizer<SSLCertificateService> T;
        private readonly ILogger<SSLCertificateService> _logger;

        public SSLCertificateService(IStringLocalizer<SSLCertificateService> stringLocalizer,
          ILogger<SSLCertificateService> logger)
        {
            _logger = logger;
            T = stringLocalizer;
        }

        public void ConfigureKestrelForSSL(KestrelServerOptions options)
        {
            var settings = GetSSLCertificateSettings();
            if (!IsValidSSLCertificateSettings(settings))
            {
                _logger.LogWarning("The SSLCertificate module is not correctly configured.");
                return;
            }
            var certificate = GetCertificate(settings.StoreLocation.Value, settings.StoreName.Value, settings.ThumbPrint);
            options.UseHttps(certificate);
        }

        public SSLCertificateSettings GetSSLCertificateSettings()
        {
            SSLCertificateSettings sslCertificateSettings = null;
            if (!File.Exists("sslsettings.json"))
                return new SSLCertificateSettings();
            using (StreamReader sr = File.OpenText("sslsettings.json"))
            {
                sslCertificateSettings = JsonConvert.DeserializeObject<SSLCertificateSettings>(sr.ReadToEnd());
            }
            return sslCertificateSettings;
        }

        public void UpdateSSLCertificateSettings(SSLCertificateSettings settings)
        {
            File.WriteAllText("sslsettings.json", JsonConvert.SerializeObject(settings));
        }

        public bool IsValidSSLCertificateSettings(SSLCertificateSettings settings, ModelStateDictionary modelState)
        {
            if (settings == null)
            {
                modelState.AddModelError("", T["Settings are not stablished."]);
                return false;
            }

            if (settings.StoreName == null)
            {
                modelState.AddModelError("StoreName", T["A Certificate Store Name is required."]);
            }
            if (settings.StoreLocation == null)
            {
                modelState.AddModelError("StoreLocation", T["A Certificate Store Location is required."]);
            }
            if (string.IsNullOrWhiteSpace(settings.ThumbPrint))
            {
                modelState.AddModelError("ThumbPrint", T["A certificate is required."]);
            }
            if (!modelState.IsValid)
                return false;

            var certificate = GetCertificateInfo(settings.StoreLocation.Value, settings.StoreName.Value, settings.ThumbPrint);
            if (certificate == null)
            {
                modelState.AddModelError("ThumbPrint", T["The certificate cannot be found."]);
                return false;
            }
            if (!certificate.HasPrivateKey)
            {
                modelState.AddModelError("ThumbPrint", T["The certificate doesn't contain the required private key."]);
                return false;
            }
            if (certificate.Archived)
            {
                modelState.AddModelError("CertificateThumbPrint", T["The certificate is not valid because it is marked as archived."]);
                return false;
            }
            var now = DateTime.Now;
            if (certificate.NotBefore > now || certificate.NotAfter < now)
            {
                modelState.AddModelError("CertificateThumbPrint", T["The certificate is not valid for current date."]);
                return false;
            }
            return modelState.IsValid;
        }

        public bool IsValidSSLCertificateSettings(SSLCertificateSettings settings)
        {
            var modelState = new ModelStateDictionary();
            return IsValidSSLCertificateSettings(settings, modelState);
        }

        public IEnumerable<CertificateInfo> GetAvailableCertificates(bool onlyCertsWithPrivateKey)
        {
            foreach (StoreLocation storeLocation in Enum.GetValues(typeof(StoreLocation)))
            {
                foreach (StoreName storeName in Enum.GetValues(typeof(StoreName)))
                {
                    using (X509Store x509Store = new X509Store(storeName, storeLocation))
                    {
                        yield return new CertificateInfo()
                        {
                            StoreLocation = storeLocation,
                            StoreName = storeName
                        };

                        x509Store.Open(OpenFlags.ReadOnly);

                        X509Certificate2Collection certificates = x509Store.Certificates;
                        foreach (var cert in certificates)
                        {
                            if (!cert.Archived && (!onlyCertsWithPrivateKey || (onlyCertsWithPrivateKey && cert.HasPrivateKey)))
                            {
                                yield return new CertificateInfo()
                                {
                                    StoreLocation = storeLocation,
                                    StoreName = storeName,
                                    FriendlyName = cert.FriendlyName,
                                    Issuer = cert.Issuer,
                                    Subject = cert.Subject,
                                    NotBefore = cert.NotBefore,
                                    NotAfter = cert.NotAfter,
                                    ThumbPrint = cert.Thumbprint,
                                    HasPrivateKey = cert.HasPrivateKey,
                                    Archived = cert.Archived
                                };
                            }
                        }
                    }
                }
            }
        }
        public CertificateInfo GetCertificateInfo(StoreLocation storeLocation, StoreName storeName, string certThumbPrint)
        {
            var cert = GetCertificate(storeLocation, storeName, certThumbPrint);
            if (cert == null)
                return null;
            return new CertificateInfo()
            {
                StoreLocation = storeLocation,
                StoreName = storeName,
                FriendlyName = cert.FriendlyName,
                Issuer = cert.Issuer,
                Subject = cert.Subject,
                NotBefore = cert.NotBefore,
                NotAfter = cert.NotAfter,
                ThumbPrint = cert.Thumbprint,
                HasPrivateKey = cert.HasPrivateKey,
                Archived = cert.Archived
            };
        }

        public X509Certificate2 GetCertificate(StoreLocation storeLocation, StoreName storeName, string certThumbPrint)
        {
            return GetCertificate(storeLocation, storeName, certThumbPrint, null, null, null, null);
        }

        public X509Certificate2 GetCertificate(StoreLocation storeLocation, StoreName storeName, string certThumbPrint,  string friendlyName, string certSubjectName, string dnsName, Nullable<DateTime> notAfter)
        {
            var result = new List<X509Certificate2>();
            using (X509Store x509Store = new X509Store(storeName, storeLocation))
            {
                x509Store.Open(OpenFlags.ReadOnly);

                X509Certificate2Collection certificates = x509Store.Certificates;
                foreach (var cert in certificates)
                {
                    if (IsCertificateMatchingNotNullParameters(certThumbPrint, friendlyName, certSubjectName, dnsName, notAfter, cert))
                    {
                        result.Add(cert);
                    }
                }
            }
            if (!result.Any())
                return null;
            return result.OrderByDescending(cert=>cert.NotAfter).FirstOrDefault();
        }

        private bool IsCertificateMatchingNotNullParameters(string certThumbPrint, string friendlyName, string certSubjectName, string dnsName, DateTime? notAfter, X509Certificate2 cert)
        {
            return (certThumbPrint == null || string.Compare(cert.Thumbprint, certThumbPrint) == 0)
                                    &&
                                    (friendlyName == null || string.Compare(cert.FriendlyName, friendlyName) == 0)
                                    &&
                                    (certSubjectName == null || string.Compare(cert.SubjectName.Name, "CN=" + certSubjectName) == 0)
                                    &&
                                    (dnsName == null || string.Compare(cert.GetNameInfo(X509NameType.DnsName, false), dnsName) == 0)
                                    &&
                                    (notAfter == null || !notAfter.HasValue || cert.NotAfter.Date == notAfter.Value.Date);
        }
    }
}