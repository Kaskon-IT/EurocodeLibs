namespace ExportFactory.Services
{
    using System.IO;

    public class ExportHelper
    {
        public static async Task WriteToStreamAsync(string content)
        {
            using var fileStream = new FileStream("Export_Grondslagen.csv", FileMode.Create, FileAccess.Write);
            using var streamWriter = new StreamWriter(fileStream);
            await streamWriter.WriteAsync(content);
        }
        public static void WriteToStream(string content)
        {
            using var fileStream = new FileStream("Export_Grondslagen.csv", FileMode.Create, FileAccess.Write);
            using var streamWriter = new StreamWriter(fileStream);
            streamWriter.Write(content);
        }

    }

}
