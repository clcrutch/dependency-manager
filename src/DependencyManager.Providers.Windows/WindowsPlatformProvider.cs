using DependencyManager.Core.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyManager.Providers.Windows
{
    public class WindowsPlatformProvider : IPlatformProvider
    {
        public string Name => "Windows";

        public Task<bool> TestAsync() =>
            Task.FromResult(OperatingSystem.IsWindows());
    }
}
