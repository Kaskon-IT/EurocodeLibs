namespace CommonLibrary.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public static class EnumerableExtensions
    {
        private class NaturalStringComparer : IComparer<string?>
        {
            private static readonly Regex _regex = new(@"\d+", RegexOptions.Compiled);

            public int Compare(string? x, string? y)
            {
                if (x == null) return y == null ? 0 : -1;
                if (y == null) return 1;

                var xParts = _regex.Split(x);
                var yParts = _regex.Split(y);
                var xNums = _regex.Matches(x);
                var yNums = _regex.Matches(y);

                int i = 0, j = 0, n = 0;

                while (i < xParts.Length && j < yParts.Length)
                {
                    int cmp = string.Compare(xParts[i], yParts[j], StringComparison.OrdinalIgnoreCase);
                    if (cmp != 0)
                        return cmp;

                    if (n < xNums.Count && n < yNums.Count)
                    {
                        int numX = int.Parse(xNums[n].Value);
                        int numY = int.Parse(yNums[n].Value);
                        if (numX != numY)
                            return numX.CompareTo(numY);
                    }

                    i++;
                    j++;
                    n++;
                }

                return x.Length.CompareTo(y.Length);
            }
        }

        private static readonly NaturalStringComparer _naturalComparer = new();

        // ----- Voor IEnumerable<string?> -----
        public static IOrderedEnumerable<string?> OrderByNatural(this IEnumerable<string?> source) =>
            source.OrderBy(s => s, _naturalComparer);

        public static IOrderedEnumerable<string?> OrderByNaturalDescending(this IEnumerable<string?> source) =>
            source.OrderByDescending(s => s, _naturalComparer);

        public static IOrderedEnumerable<string?> ThenByNatural(this IOrderedEnumerable<string?> source) =>
            source.ThenBy(s => s, _naturalComparer);

        public static IOrderedEnumerable<string?> ThenByNaturalDescending(this IOrderedEnumerable<string?> source) =>
            source.ThenByDescending(s => s, _naturalComparer);

        // ----- Voor IEnumerable<T> met nullable selector -----
        public static IOrderedEnumerable<T> OrderByNatural<T>(this IEnumerable<T> source, Func<T, string?> selector) =>
            source.OrderBy(selector, _naturalComparer);

        public static IOrderedEnumerable<T> OrderByNaturalDescending<T>(this IEnumerable<T> source, Func<T, string?> selector) =>
            source.OrderByDescending(selector, _naturalComparer);

        public static IOrderedEnumerable<T> ThenByNatural<T>(this IOrderedEnumerable<T> source, Func<T, string?> selector) =>
            source.ThenBy(selector, _naturalComparer);

        public static IOrderedEnumerable<T> ThenByNaturalDescending<T>(this IOrderedEnumerable<T> source, Func<T, string?> selector) =>
            source.ThenByDescending(selector, _naturalComparer);

        // ----- Losse comparer voor List<T>.Sort / SortedSet<T> -----
        public static IComparer<T> NaturalComparerBy<T>(Func<T, string?> selector) =>
            Comparer<T>.Create((a, b) => _naturalComparer.Compare(selector(a), selector(b)));

        // ----- Multi-field comparer voor List<T>.Sort -----
        public static IComparer<T> NaturalMultiComparerBy<T>(params Func<T, string?>[] selectors)
        {
            if (selectors == null || selectors.Length == 0)
                throw new ArgumentException("Minstens één selector moet opgegeven worden.", nameof(selectors));

            return Comparer<T>.Create((a, b) =>
            {
                foreach (var selector in selectors)
                {
                    int cmp = NaturalComparerBy(selector).Compare(a, b);
                    if (cmp != 0) return cmp;
                }
                return 0;
            });
        }
    }



}






