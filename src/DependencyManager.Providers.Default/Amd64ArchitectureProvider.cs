using DependencyManager.Core.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyManager.Providers.Default
{
    public class Amd64ArchitectureProvider : IArchitectureProvider
    {
        public string Name => "amd64";

        public Task<bool> TestAsync() =>
            Task.FromResult(Environment.Is64BitOperatingSystem);
    }
}
