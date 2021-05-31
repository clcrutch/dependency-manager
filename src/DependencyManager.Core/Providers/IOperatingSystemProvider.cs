using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyManager.Core.Providers
{
    public interface IOperatingSystemProvider
    {
        Task<string> GetFullExecutablePathAsync(string executable);
        Task<bool> IsUserAdminAsync();
    }
}
