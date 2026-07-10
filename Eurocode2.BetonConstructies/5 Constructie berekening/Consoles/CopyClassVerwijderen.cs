using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eurocode.BetonConstructies
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public enum J3ConsoleLinkType
    {
        Geen,
        HorizontaalOfSchuin,
        Verticaal
    }

    public enum J3NodeType
    {
        CCC,
        CCT,
        CTT
    }

    public class J3ConsoleNodeResult
    {
        public string Naam { get; set; } = "";
        public J3NodeType Type { get; set; }

        public double X { get; set; }              // mm
        public double Y { get; set; }              // mm

        public double SigmaEd { get; set; }        // N/mm²
        public double SigmaRdMax { get; set; }     // N/mm²

        public double UnityCheck =>
            SigmaRdMax > 0 ? SigmaEd / SigmaRdMax : double.PositiveInfinity;

        public bool IsOk => UnityCheck <= 1.0;
    }
}
