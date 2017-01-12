using Microsoft.Extensions.Localization;
using Orchard.Environment.Commands;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.DependencyInjection;
using Orchard.SSLCertificate.Services;
using Orchard.SSLCertificate.Settings;
using System.Security.Cryptography.X509Certificates;
using Orchard.OpenId.Services;
using System.Collections.Generic;

namespace Orchard.SSLCertificate.Commands
{
    public class SSLCertificateCommands : DefaultCommandHandler
    {
        private readonly ISSLCertificateService _sslCertificateService;
        private readonly IOpenIdService _openIdService;

        public SSLCertificateCommands(ISSLCertificateService sslCertificateService,
                            IStringLocalizer<SSLCertificateCommands> localizer,
                            IEnumerable<IOpenIdService> openIdServices) : base(localizer)
        {
            _sslCertificateService = sslCertificateService;
            _openIdService = openIdServices.FirstOrDefault();
            CertStoreLocation = StoreLocation.CurrentUser;
            CertStoreName = StoreName.My;
        }

        [OrchardSwitch]
        public StoreLocation CertStoreLocation { get; set; }

        [OrchardSwitch]
        public StoreName CertStoreName { get; set; }

        [OrchardSwitch]
        public string FriendlyName { get; set; }

        [OrchardSwitch]
        public string SubjectName { get; set; }

        [OrchardSwitch]
        public string DnsName { get; set; }

        [OrchardSwitch]
        public DateTime ExpirationDate { get; set; }

        [CommandName("configureSSLCertificate")]
        [CommandHelp("configureSSLCertificate /CertStoreLocation:[CurrentUser|LocalMachine] /CertStoreName:[AddressBook|AuthRoot|My] /FriendlyName:<FriendlyName> /SubjectName:<SubjectName> /DnsName:<DnsName> /ExpirationDate:<ExpirationDate>\r\n\t" + "Sets an existing certificate for SSL config")]
        [OrchardSwitches("CertStoreLocation,CertStoreName,FriendlyName,SubjectName,DnsName,ExpirationDate")]
        public void ConfigureSSLCertificate()
        {
            if (IsValidStoreName())
                return;

            var certificate = _sslCertificateService.GetCertificate(CertStoreLocation, CertStoreName, null, FriendlyName, SubjectName, DnsName, ExpirationDate);
            if (certificate == null)
            {
                Context.Output.WriteLine(T["Certificate not found"]);
                return;
            }
            Context.Output.WriteLine(T["Certificate with thumbprint:{0} found", certificate.Thumbprint]);

            var settings = new SSLCertificateSettings();
            settings.StoreLocation = CertStoreLocation;
            settings.StoreName = CertStoreName;
            settings.ThumbPrint = certificate.Thumbprint;
            
            _sslCertificateService.UpdateSSLCertificateSettings(settings);
            Context.Output.WriteLine(T["Updated SSL Settings with the new certificate"]);
        }
        
        [CommandName("configureOpenIDCertificate")]
        [CommandHelp("configureOpenIDCertificate /CertStoreLocation:[CurrentUser|LocalMachine] /CertStoreName:[AddressBook|AuthRoot|My] /FriendlyName:<FriendlyName> /SubjectName:<SubjectName> /DnsName:<DnsName> /ExpirationDate:<ExpirationDate>\r\n\t" + "Creates a new Certificate for OpenID Connect Settings")]
        [OrchardSwitches("CertStoreLocation,CertStoreName,FriendlyName,SubjectName,DnsName,ExpirationDate")]
        public void ConfigureOpenIDCertificate()
        {
            if (_openIdService == null)
            {
                Context.Output.WriteLine(T["Orchard.OpenID Connect module is not enabled"]);
                return;
            }

            if (IsValidStoreName())
                return;

            var certificate = _sslCertificateService.GetCertificate(CertStoreLocation, CertStoreName, null, FriendlyName, SubjectName, DnsName, ExpirationDate);
            if (certificate == null)
            {
                Context.Output.WriteLine(T["Certificate not found"]);
                return;
            }
            Context.Output.WriteLine(T["Certificate with thumbprint:{0} found", certificate.Thumbprint]);

            var settings = _openIdService.GetOpenIdSettingsAsync().GetAwaiter().GetResult();
            settings.CertificateStoreLocation = CertStoreLocation;
            settings.CertificateStoreName = CertStoreName;
            settings.CertificateThumbPrint = certificate.Thumbprint;

            _openIdService.UpdateOpenIdSettingsAsync(settings);
            Context.Output.WriteLine(T["Updated OpenId Connect Settings with the new certificate"]);
        }

        private bool IsValidStoreName()
        {
            if (CertStoreName != StoreName.AddressBook && CertStoreName != StoreName.AuthRoot && CertStoreName != StoreName.My)
            {
                Context.Output.WriteLine(T["Invalid CertStoreName parameter, only [AddressBook|AuthRoot|My] are valid parameters"]);
                return false;
            }
            return true;
        }
    }
}