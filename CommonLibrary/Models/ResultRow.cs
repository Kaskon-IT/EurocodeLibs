using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Models
{
    public class ResultRow
    {
        public string Toelichting { get; set; } = "";
        public string SymboolHtml { get; set; } = "";
        public string SymboolTex { get; set; } = "";
        public string Waarde { get; set; } = "";
        public string Eenheid { get; set; } = "";
        public string? Artikel { get; set; }
        public string? FormuleTex { get; set; }
        public bool? IsOk { get; set; }
    }

    public interface IRowResult
    {
        List<ResultRow> ResultRows { get; }
    }
}
