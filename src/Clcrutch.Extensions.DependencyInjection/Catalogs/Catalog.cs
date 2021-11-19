using Clcrutch.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Composition;

namespace Clcrutch.Extensions.DependencyInjection.Catalogs
{
    public abstract class Catalog
    {
        protected IServiceCollection ServiceCollection { get; }

        protected Catalog()
            : this(new ServiceCollection()) { }

        protected Catalog(IServiceCollection serviceCollection)
        {
            ServiceCollection = serviceCollection;
        }

        public async Task<IServiceProvider> GetServiceProvider()
        {
            var types = await GetContainedTypesAsync();
            foreach (var type in types)
            {
                var attributes = type.GetCustomAttributes(typeof(ExportAttribute), true);

                if (attributes is ExportAttribute[] exportAttributes)
                {
                    foreach (var exportAttribute in exportAttributes)
                    {
                        if (exportAttribute.ContractType != null)
                        {
                            ServiceCollection.AddTransient(exportAttribute.ContractType, type);
                        }
                        else
                        {
                            ServiceCollection.AddTransient(type);
                        }
                    }
                }
            }

            return ServiceCollection.BuildServiceProvider();
        }

        protected internal abstract Task<IEnumerable<Type>> GetAvailableTypesAsync();

        protected virtual async Task<IEnumerable<Type>> GetContainedTypesAsync() =>
            await GetAvailableTypesAsync()
                .Cast()
                .Where(t => t.CustomAttributes.Any(x => x.AttributeType.IsAssignableTo(typeof(ExportAttribute))))
                .Where(t => TestOperatingSystemAsync(t))
                .ToArrayAsync();

        private async Task<bool> TestOperatingSystemAsync(Type containedType)
        {
            if (containedType.CustomAttributes.All(x => !x.AttributeType.IsAssignableTo(typeof(OperatingSystemRequiredAttribute))))
            { 
                return true;
            }

            var operatingSystemCheckerTypes = (from a in containedType.GetCustomAttributes(typeof(OperatingSystemRequiredAttribute), true).Select(x => (OperatingSystemRequiredAttribute)x)
                                               select a.OperatingSystemCheckerTypes)
                                               .SelectMany(x => x)
                                               .ToArray();

            var sync = from m in operatingSystemCheckerTypes.SelectMany(t => t.GetMethods())
                       where (m.Name.Equals("Test", StringComparison.OrdinalIgnoreCase) || m.Name.Equals("Check", StringComparison.OrdinalIgnoreCase)) &&
                                m.ReturnType.IsAssignableTo(typeof(bool))
                       select (bool?)m.Invoke(Activator.CreateInstance(m.DeclaringType ?? typeof(object)), null) ?? false;

            var async = await Task.WhenAll(from m in operatingSystemCheckerTypes.SelectMany(t => t.GetMethods())
                                           where (m.Name.Equals("Test", StringComparison.OrdinalIgnoreCase) || m.Name.Equals("Check", StringComparison.OrdinalIgnoreCase) ||
                                                   m.Name.Equals("TestAsync", StringComparison.OrdinalIgnoreCase) || m.Name.Equals("CheckAsync", StringComparison.OrdinalIgnoreCase)) &&
                                                    m.ReturnType.IsAssignableTo(typeof(Task<bool>))
                                           select (Task<bool>?)m.Invoke(Activator.CreateInstance(m.DeclaringType ?? typeof(object)), null) ?? Task.FromResult<bool>(false));

            return sync.Concat(async).Any(x => x);
        }
    }
}
