using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clcrutch.Linq
{
    public static class IAsyncEnumerable
    {
        public static async IAsyncEnumerable<TResult> Select<TSource, TResult>(this IAsyncEnumerable<TSource> @this, Func<TSource, TResult> selector)
        {
            var enumerator = @this.GetAsyncEnumerator();
            while (await enumerator.MoveNextAsync())
            {
                yield return selector(enumerator.Current);
            }
        }

        public static async IAsyncEnumerable<TResult> SelectMany<TSource, TResult>(this IAsyncEnumerable<TSource> @this, Func<TSource, IEnumerable<TResult>> selector)
        {
            var enumerator = @this.GetAsyncEnumerator();
            while (await enumerator.MoveNextAsync())
            {
                foreach (var item in selector(enumerator.Current))
                {
                    yield return item;
                }
            }
        }

        public static async Task<T[]> ToArrayAsync<T>(this IAsyncEnumerable<T> @this)
        {
            var list = await @this.ToListAsync();
            return list.ToArray();
        }

        public static async Task<IList<T>> ToListAsync<T>(this IAsyncEnumerable<T> @this)
        {
            var list = new List<T>();

            var enumerator = @this.GetAsyncEnumerator();
            while (await enumerator.MoveNextAsync())
            {
                list.Add(enumerator.Current);
            }

            return list;
        }
    }
}
