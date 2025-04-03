namespace Eurocode.Blazor.Demo.Shared.Helpers
{
    public class NaturalStringComparer : IComparer<string?>
    {
        public int Compare(string? x, string? y)
        {
            if (x == null) return y == null ? 0 : -1;
            if (y == null) return 1;
            return StringLogicalComparer.Compare(x, y);
        }
    }

    // Helper voor natuurlijke sortering
    public static class StringLogicalComparer
    {
        [System.Runtime.InteropServices.DllImport("shlwapi.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, ExactSpelling = true)]
        private static extern int StrCmpLogicalW(string x, string y);

        public static int Compare(string x, string y) => StrCmpLogicalW(x, y);
    }
}
