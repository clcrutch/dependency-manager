using DependencyManager.Core.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace DependencyManager.Providers.Windows
{
    public class WindowsOperatingSystemProvider : IOperatingSystemProvider
    {
        public Task<bool> IsUserAdminAsync()
        {
            if (OperatingSystem.IsWindows())
            {
                using var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return Task.FromResult(principal.IsInRole(WindowsBuiltInRole.Administrator));
            }

            throw new NotSupportedException();
        }
    }
}
