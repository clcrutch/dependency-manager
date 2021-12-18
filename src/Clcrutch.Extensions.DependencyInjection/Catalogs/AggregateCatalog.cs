using Clcrutch.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Reflection;

namespace Clcrutch.Extensions.DependencyInjection.Catalogs
{
    public class AggregateCatalog : Catalog, IList<Catalog>
    {
        public override string Name => string.Join(", ", Catalogs.Select(x => x.Name));

        protected List<Catalog> Catalogs { get; }

        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public Catalog this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public AggregateCatalog(params Catalog[] catalogs)
            : this(new ServiceCollection(), catalogs) { }

        public AggregateCatalog(IServiceCollection serviceCollection, params Catalog[] catalogs)
            : base(serviceCollection)
        {
            Catalogs = catalogs.ToList();
        }

        public int IndexOf(Catalog item) =>
            Catalogs.IndexOf(item);

        public void Insert(int index, Catalog item)
        {
            ServiceProvider = null;
            Catalogs.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ServiceProvider = null;
            Catalogs.RemoveAt(index);
        }

        public void Add(Catalog item)
        {
            ServiceProvider = null;
            Catalogs.Add(item);
        }

        public void Clear()
        {
            ServiceProvider = null;
            Catalogs.Clear();
        }

        public bool Contains(Catalog item) =>
            Catalogs.Contains(item);

        public void CopyTo(Catalog[] array, int arrayIndex)
        {
            ServiceProvider = null;
            Catalogs.CopyTo(array, arrayIndex);
        }

        public bool Remove(Catalog item)
        {
            ServiceProvider = null;
            return Catalogs.Remove(item);
        }

        public IEnumerator<Catalog> GetEnumerator() =>
            Catalogs.GetEnumerator();

        protected internal override async Task<IEnumerable<Type>> GetAvailableTypesAsync() =>
            (await Task.WhenAll(from c in Catalogs
                                select c.GetAvailableTypesAsync())).SelectMany(x => x).ToList();

        IEnumerator IEnumerable.GetEnumerator() =>
            Catalogs.GetEnumerator();
    }
}
