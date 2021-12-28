using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Clcrutch.Extensions.DependencyInjection.Catalogs
{
    public class NuGetAssemblyCatalog : AssemblyCatalogBase
    {
        public override string Name => throw new NotImplementedException();

        public NuGetAssemblyCatalog(string? package, string? version, string? url)
        {

        }

        protected override Task<IEnumerable<Assembly>> GetAssembliesAsync()
        {
            throw new NotImplementedException();
        }
    }
}
