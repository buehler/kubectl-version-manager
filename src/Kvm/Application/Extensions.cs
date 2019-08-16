using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Semver;

namespace Kvm.Application
{
    internal static class Extensions
    {
        public static async Task<IList<TSource>> ToListAsync<TSource>(this IAsyncEnumerable<TSource> enumerable)
        {
            var list = new List<TSource>();
            await foreach (var element in enumerable)
            {
                list.Add(element);
            }

            return list;
        }

        public static SemVersion? MaxSatisfying(this IEnumerable<SemVersion> list, string version) =>
            list
                .Where(v => v.ToString().StartsWith(version))
                .OrderByDescending(v => v)
                .FirstOrDefault();
    }
}
