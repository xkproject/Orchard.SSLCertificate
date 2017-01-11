using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Orchard.SSLCertificate.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace Orchard.SSLCertificate.Extensions
{
    static public class WebHostBuilderKestrelExtensions
    {
        public static IWebHostBuilder UseKestrelWithSSL(this IWebHostBuilder hostBuilder)
        {
            ISSLCertificateService sslCertificateService = new SSLCertificateService(new NullStringLocalizer<SSLCertificateService>(), new NullLogger<SSLCertificateService>());
            var settings = sslCertificateService.GetSSLCertificateSettings();
            X509Certificate2 certificate = null;
            if (sslCertificateService.IsValidSSLCertificateSettings(settings))
                certificate = sslCertificateService.GetCertificate(settings.StoreLocation.Value, settings.StoreName.Value, settings.ThumbPrint);

            if (certificate == null)
                return hostBuilder.UseKestrel();
            return hostBuilder.UseKestrel(options => options.UseHttps(certificate));
        }
        
        #region Fake Services
        public class NullLogger : ILogger
        {
            public IDisposable BeginScope<TState>(TState state)
            {
                return new FakeScope();
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return false;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
            }

            private class FakeScope : IDisposable
            {
                public void Dispose()
                {
                }
            }
        }
        public class NullLogger<T> : NullLogger, ILogger<T> { }
        public class NullStringLocalizer<T> : IStringLocalizer<T>
        {
            LocalizedString IStringLocalizer.this[string name] { get { return new LocalizedString(name, name); } }
            LocalizedString IStringLocalizer.this[string name, params object[] arguments] { get { return new LocalizedString(name, string.Format(name, arguments)); } }
            IEnumerable<LocalizedString> IStringLocalizer.GetAllStrings(bool includeParentCultures)
            {
                return new List<LocalizedString>();
            }
            IStringLocalizer IStringLocalizer.WithCulture(CultureInfo culture)
            {
                return this;
            }
        }
        #endregion
    }
}
