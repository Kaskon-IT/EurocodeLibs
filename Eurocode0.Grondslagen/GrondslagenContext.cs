
using CsvFactory;
using CsvFactory.Interfaces;
using MigraDoc.DocumentObjectModel;

namespace Eurocode.Grondslagen
{
    public class GrondslagenContext : IExportableCsv, IExportableRtf, IExportableMigraDoc
    {

        public NationaleBijlageEnum? NationaleBijlage { get; set; } = NationaleBijlageEnum.NL;

        /// <summary>
        /// B.3.1 Gevolgklassen
        /// (1) Ten behoeve van de betrouwbaarheidsdifferentiatie, mogen gevolgklassen (CC), zoals gegeven in tabel B1, 
        /// worden gedefinieerd door het beschouwen van de gevolgen van bezwijken of het slecht functioneren van de
        /// constructie
        /// </summary>
        public GevolgklasseEnum? Gevolgklasse { get; set; } = GevolgklasseEnum.CC2;

        public OntwerpLevensduurEnum? OntwerpLevensduur { get; set; } = OntwerpLevensduurEnum.Vijftig;




        public BetrouwbaarheidsklasseEnum Betrouwbaarheidsklasse
        {
            get { return this.Gevolgklasse.GetBetrouwbaarheidsklasse(); }
        }


        /// <summary>
        /// B.3.3
        /// Vermenigvuldigingsfactor KFI die wordt toegepast op de partiele factoren.
        /// </summary>
        public double Kfi
        {
            get { return this.Betrouwbaarheidsklasse.GetKfi(); }
        }

        /// <summary>
        /// ξ (xi) is een reductiefactor voor ongunstige, blijvende belastingen G
        /// Deze wordt gebruikt in de fundamentele combinatie (6.10b) en is afhankelijk van de nationale bijlage.
        /// </summary>
        public double Xi
        {
            get { return this.NationaleBijlage.GetReductieFactorVoorOngunstigeBlijvendeBelastingen(); }
        }


        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>()
        {
            { "NationaleBijlage", "Eurocode [NB]"},
            { "Gevolgklasse", "Gevolgklasse [CC]" },
            { "OntwerpLevensduur", "Levensduur [jaren]"},
            { "Betrouwbaarheidsklasse", "Reliability Class RC" },
            { "Kfi", "K_FI~" },
            { "Xi", "ξ" },
        };

        public Dictionary<string, string> RowData { get { return this.GetRowData(); } }


        public void AddToSection(Section section)
        {


            throw new NotImplementedException();
        }

        //public string CreateCsv()
        //{
        //    var creator = new CsvFileCreator();
        //    creator.SetColumnHeaders(Headers);
        //    creator.AddRow(RowData);
        //    var stringBuilder = creator.GetCsvStringBuilder();
        //    return stringBuilder.ToString();

        //    throw new NotImplementedException();
        //}



        string IExportableRtf.Export()
        {

            throw new NotImplementedException();

        }




        string IExportableCsv.Export()
        {
            var creator = new CsvFileCreator();
            creator.SetColumnHeaders(Headers);
            creator.AddRow(RowData);
            var stringBuilder = creator.GetCsvStringBuilder();
            return stringBuilder.ToString();
        }


    }
}
