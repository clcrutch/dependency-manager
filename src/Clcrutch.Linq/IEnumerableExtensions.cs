namespace Clcrutch.Linq
{
    public static class IEnumerableExtensions
    {
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
