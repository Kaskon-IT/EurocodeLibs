using CommonLibrary;
using ExportFactory.MigraDocContentModels;
using Microsoft.AspNetCore.Components;
using MigraDoc.DocumentObjectModel.Tables;
using System.Data;

namespace ExportFactory.Extensions
{
    public static class BaseEurocodeContextExtensions
    {
        // Convert 

        public static DataTable ToDataTable<T>(this T obj) where T : BaseEurocodeContext
        {
            // maak een list (ook al zit er maar 1 item in)
            var list = new List<T> { obj };
            // gebruik de extensie methode om de list om te zetten naar een DataTable
            return list.ToDataTable();
        }



        public static MarkupString ToHtmlTable<T>(this T obj, bool isDraaiTabel = true) where T : BaseEurocodeContext
        {
            var dt = obj.ToDataTable(); // datatable

            // zijn er meldingen?
            //if (obj.Meldingen.Any())

            var mdd = dt.ToMigraDocDocument(objectType: obj.GetType(), isPivotTable: isDraaiTabel, meldingen: [.. obj.Meldingen.OrderBy(m => m.Code)]); // migraDoc.Document

            // onderaan alle opmerkingen (indien aanwezig)





            var html = ExportFactory.Services.HtmlCreator.GenerateHtmlFromDocument(mdd);





            return new MarkupString(html);
        }



        public static MigraDocTable ToMigraDocTable<T>(this T obj, bool isDraaiTabel = true) where T : BaseEurocodeContext
        {
            var dt = obj.ToDataTable(); // start met datatable
            var mdt = dt.ToTable(obj.GetType(), isPivotTable: isDraaiTabel); // migraDocTable
            return new MigraDocTable { Table = mdt ?? new MigraDoc.DocumentObjectModel.Tables.Table() };
        }

        public static MigraDocTable ToMigraDocTable(this IEnumerable<BaseEurocodeContext> list, Type type, bool isDraaiTabel = true)
        {
            ArgumentNullException.ThrowIfNull(list);
            ArgumentNullException.ThrowIfNull(type);

            DataTable dataTable = list.ToDataTable(); // jouw extensie op IEnumerable<BaseEurocodeContext>

            // Controleer of allemaal gelijke afgeleide types.
            var distinctTypes = list.Select(x => x.GetType()).Distinct().ToList();
            if (distinctTypes.Count > 1)
                throw new InvalidOperationException("De lijst bevat meerdere afgeleide types. Specificeer expliciet een type.");

            // Bepaal het concrete type als dit niet expliciet is meegegeven
            Type actualType = type ?? list.FirstOrDefault()?.GetType()
                ?? throw new ArgumentException("Type kon niet worden afgeleid uit de lijst.", nameof(type));

            Table? table = dataTable.ToTable(actualType, isDraaiTabel);

            return new MigraDocTable
            {
                Table = table ?? new Table()
            };






        }


        public static void AddToSection<T>(this IEnumerable<T> list, SectionContent section, bool isDraaiTabel = true, string? title = null, string style = "")
            where T : BaseEurocodeContext
        {
            ArgumentNullException.ThrowIfNull(list);


            // controleer of de lijst alleen unieke contexten zijn, of groepeer ze
            var groupedList = list
                .Where(x => x != null)
                .GroupBy(x => x.GetType())
                .ToList();

            foreach (var grouped in groupedList)
            {
                if (grouped.FirstOrDefault() == null)
                    continue;

                var type = grouped.FirstOrDefault()?.GetType();

                if (type == null)
                    continue;




                grouped.AddToSection(section, type, isDraaiTabel, title, style);
            }



        }



        public static void AddToSection(this IEnumerable<BaseEurocodeContext> list, SectionContent section, Type type, bool isDraaiTabel = true, string? title = null, string style = "")
        {
            if (list == null || !list.Any())
                return;

            // optionele algemene titel
            if (!string.IsNullOrWhiteSpace(title))
            {
                section.AddParagraph(title, style);
            }
            else if (list.First()?.Heading != null)
            {
                section.AddParagraph(list.First()?.Heading ?? "NO HEADING", style);
            }




            // maak een migraDocTable
            var mdt = list.ToMigraDocTable(type, isDraaiTabel);
            section.Elements.Add(mdt); // voeg de tabel toe aan de sectie

            // Verzamel alle meldingen
            // Verzamel alle unieke meldingen
            var meldingen = list
                .SelectMany(item => item.Meldingen)
                .Distinct() // vereist correcte Equals/GetHashCode op Melding
                .OrderBy(m => m.Code)
                .ToList();

            if (meldingen.Any())
            {
                var markdown = string.Join("\n", meldingen.Select(m => m.ToMarkDownString()));
                section.Elements.Add(new ParagraphContent { Markdown = markdown });
            }
        }


        public static void AddToSection<T>(this T obj, SectionContent section, bool isDraaiTabel = true, string title = "", string style = "") where T : BaseEurocodeContext
        {
            // heading
            if (!string.IsNullOrEmpty(title))
            {
                section.AddParagraph(title, style);
            }

            // maak een MigraDocTable van de context
            var table = obj.ToMigraDocTable(isDraaiTabel);

            section.Elements.Add(table); // voeg de tabel toe aan de sectie

            // voeg de meldingen toe als paragrafen
            var par = obj.ToParagraphContent();
            if (par != null)
            {
                section.Elements.Add(par); // voeg de paragrafen toe aan de sectie
            }
        }



        private static ParagraphContent? ToParagraphContent<T>(this T obj) where T : BaseEurocodeContext
        {
            if (!obj.Meldingen.Any())
                return null;

            string prefix;
            string markdownText;

            if (obj.Meldingen.Count == 1)
            {
                prefix = "Opmerking\n";
                markdownText = prefix + obj.Meldingen.First().ToMarkDownString();
            }
            else
            {
                prefix = "Opmerkingen\n";
                markdownText = prefix + string.Join("\n", obj.Meldingen
                    .OrderBy(m => m.Code)
                    .Select(m => $"> {m.ToMarkDownString()}"));
            }

            return new ParagraphContent
            {
                Markdown = markdownText
            };

        }

    }


}
