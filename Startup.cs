using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Server.Kestrel;
using Orchard.Environment.Navigation;
using Orchard.SSLCertificate.Drivers;
using Orchard.Settings.Services;
using Orchard.SSLCertificate.Services;
using Orchard.SSLCertificate.Commands;
using Orchard.Environment.Commands;
using Microsoft.AspNetCore.Modules;

namespace Orchard.SSLCertificate
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ISSLCertificateService, SSLCertificateService>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<ISiteSettingsDisplayDriver, SSLCertificateSettingsDisplayDriver>();
            services.AddScoped<ICommandHandler, SSLCertificateCommands>();
        }
    }
}
