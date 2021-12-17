using Clcrutch.Linq;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Reflection;

namespace Clcrutch.Extensions.DependencyInjection.Catalogs
{
    public abstract class AssemblyCatalogBase : Catalog
    {
        public AssemblyCatalogBase()
            : base() { }

        public AssemblyCatalogBase(IServiceCollection serviceCollection)
            : base(serviceCollection) { }

        protected abstract Task<IEnumerable<Assembly>> GetAssembliesAsync();

        protected internal override async Task<IEnumerable<Type>> GetAvailableTypesAsync()
        {
            var types = await GetAssembliesAsync()
                            .Cast()
                            .Select(assembly => assembly.GetTypes())
                            .SelectMany(t => t)
                            .ToListAsync();

            Log.Debug("{types} were found in {name}.", types, Name);

            return types;
        }
    }
}
