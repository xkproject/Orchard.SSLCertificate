using Microsoft.Extensions.Localization;
using Orchard.Environment.Navigation;
using Orchard.Environment.Shell;
using System;

namespace Orchard.SSLCertificate
{
    public class AdminMenu : INavigationProvider
    {
        private readonly ShellSettings _shellSettings;
        public AdminMenu(IStringLocalizer<AdminMenu> localizer, ShellSettings shellSettings)
        {
            T = localizer;
            _shellSettings = shellSettings;
        }

        public IStringLocalizer T { get; set; }

        public void BuildNavigation(string name, NavigationBuilder builder)
        {
            if (_shellSettings.Name != "Default" || !String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            builder
                .Add(T["Design"], design => design
                    .Add(T["Settings"], settings => settings
                        .Add(T["SSL Certificate"], "11", entry => entry
                            .Action("Edit", "Admin", "Orchard.SSLCertificate")
                            .LocalNav()
                        )
                    )
                );
        }
    }
}
