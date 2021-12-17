using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Clcrutch.Extensions.DependencyInjection.Catalogs
{
    public class AssemblyCatalog : AssemblyCatalogBase
    {
        public Assembly Assembly { get; set; }

        public override string Name => Assembly.GetName().Name;

        public AssemblyCatalog(Assembly assembly)
            : this(assembly, new ServiceCollection())
        {
        }

        public AssemblyCatalog(Assembly assembly, IServiceCollection serviceCollection)
            : base(serviceCollection)
        {
            Assembly = assembly;
        }

        protected override async Task<IEnumerable<Assembly>> GetAssembliesAsync() =>
            await Task.FromResult(new List<Assembly>()
                    {
                        Assembly
                    });
    }
}
