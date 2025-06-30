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
            var mdd = dt.ToMigraDocDocument(objectType: obj.GetType(), isPivotTable: isDraaiTabel); // migraDoc.Document
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
            Table? table = dataTable.ToTable(type, isDraaiTabel);

            return new MigraDocTable
            {
                Table = table ?? new Table()
            };
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
            if (obj.Meldingen.Any())
            {
                ParagraphContent par = new ParagraphContent
                {
                    Markdown = obj.Meldingen.Select(m => m.ToMarkupString().Value).Aggregate((current, next) => current + "\n" + next),
                };
                return par;
            }
            else return null;

        }

    }


}
