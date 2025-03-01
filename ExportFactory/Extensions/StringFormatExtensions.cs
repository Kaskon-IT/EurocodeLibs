namespace ExportFactory.Extensions
{
    public static class StringFormatExtensions
    {

        //public enum DefaultStringFormatEnum
        //{
        //    Procent,
        //    Promille,

        //}

        //public static string GetDefaultStringFormat()
        //{

        //}

        //public static string GetStringFormat()

        public static string GetFormattedStringPromille(this double value, string format = "0.##")
        {
            //value *= 1000;
            format += "\t‰";
            return value.ToString(format);
        }

        public static string GetFormattedStringProcent(this double value, string format = "0.##")
        {
            //value *= 100;
            format += "\t%";
            return value.ToString(format);
        }


        public static string GetFormattedStringValue(this double value, string? eenheid = null, string format = "e2")
        {

            if (eenheid != null)
                format += $"\t{eenheid}";

            return value.ToString(format);


        }
    }
}
