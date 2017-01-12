﻿using System;
using System.Security.Cryptography.X509Certificates;

namespace Orchard.SSLCertificate.ViewModels
{
    public class CertificateInfo
    {
        public string FriendlyName { get; set; }
        public string Issuer { get; set; }
        public DateTime NotAfter { get; set; }
        public DateTime NotBefore { get; set; }
        public StoreLocation StoreLocation { get; set; }
        public StoreName StoreName { get; set; }
        public string Subject { get; set; }
        public string ThumbPrint { get; set; }
        public bool HasPrivateKey { get; set; }
        public bool Archived { get; set; }
    }
}