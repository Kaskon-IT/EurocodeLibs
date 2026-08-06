namespace Eurocode.BetonConstructies
{
    using CommonLibrary.Extensions;
    using CommonLibrary.Models;
    using ExportFactory.Shared;

    /// <summary>
    /// 6.1 (10) Voor gedrongen constructies, zoals de in (1)P beschreven discontinue gebieden,
    /// mag de grootte van de inwendige hefboomsarm (z) zijn afgeleid uit:
    /// Voor consoles, tanden en andere uitkragingen:
    /// z = 0.4 a + 0.4 <= 1.6 a  /// 
    /// </summary>
    public class GedrongenUitkragingResult : IRowResult
    {
        public List<ResultRow> ResultRows { get; } = [];
        public double Z { get; set; }
        public Formula ZFormula => new Formula()
        {
            Name = "6.1 (10)",  
            StaticValue = @"z= 0.4 a + 0.4 h \leq 1.6 a",
            DynamicValue = @$"= 0.4 \cdot {A.ToTeX()} + 0.4 \cdot {H.ToTeX()} \leq 1.6 \cdot {A.ToTeX()} = {Z.ToTeX()}"
        };


        public double A { get; internal set; }
        public double H { get; internal set; }

    }
}
