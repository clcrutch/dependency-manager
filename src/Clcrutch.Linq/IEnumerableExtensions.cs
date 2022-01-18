namespace Clcrutch.Linq
{
    public static class IEnumerableExtensions
    {
        public static async IAsyncEnumerable<T> Concat<T> (this IEnumerable<T> first, IAsyncEnumerable<T> second)
        {
            foreach (var item in first)
            {
                yield return item;
            }

            var enumerator = second.GetAsyncEnumerator();
            while (await enumerator.MoveNextAsync())
            {
                yield return enumerator.Current;
            }
        }

        public static async IAsyncEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> @this, Func<TSource, Task<TResult>> selector)
        {
            foreach (var item in @this)
            {
                yield return await selector(item);
            }
        }

        public static async IAsyncEnumerable<T> Where<T>(this IEnumerable<T> @this, Func<T, Task<bool>> predicate)
        {
            foreach (var item in @this)
            {
                if (await predicate(item))
                {
                    yield return item;
                }
            }
        }
    }
}
