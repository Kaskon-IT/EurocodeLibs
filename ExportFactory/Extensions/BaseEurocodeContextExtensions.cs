using CommonLibrary;
using ExportFactory.MigraDocContentModels;
using Microsoft.AspNetCore.Components;
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


    }


}
