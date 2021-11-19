using Clcrutch.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Clcrutch.Extensions.DependencyInjection.Catalogs
{
    public class AggregateCatalog : Catalog
    {
        protected Catalog[] Catalogs { get; }

        public AggregateCatalog(params Catalog[] catalogs)
            : this(new ServiceCollection(), catalogs) { }

        public AggregateCatalog(IServiceCollection serviceCollection, params Catalog[] catalogs)
            : base(serviceCollection)
        {
            Catalogs = catalogs;
        }

        protected internal override async Task<IEnumerable<Type>> GetAvailableTypesAsync() =>
            (await Task.WhenAll(from c in Catalogs
                                select c.GetAvailableTypesAsync())).SelectMany(x => x).ToList();
    }
}
