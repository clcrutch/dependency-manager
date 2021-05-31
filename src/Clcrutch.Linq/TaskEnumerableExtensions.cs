using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clcrutch.Linq
{
    public static class TaskEnumerableExtensions
    {
        public static async IAsyncEnumerable<T> Cast<T>(this Task<IEnumerable<T>> @this)
        {
            foreach (var item in await @this)
            {
                yield return item;
            }
        }
    }
}
