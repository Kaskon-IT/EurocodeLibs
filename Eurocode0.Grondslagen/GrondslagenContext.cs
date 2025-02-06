
//using CsvFactory;
//using CsvFactory.Interfaces;

///using CsvFactory;
using ExportFactory.Shared;
using MigraDoc.DocumentObjectModel;

namespace Eurocode.Grondslagen
{
    public class GrondslagenContext
    {
        /// <summary>
        /// Synchronous CSV Export 
        /// </summary>
        /// <param name="excludeHeaders">Optional without headers</param>
        /// <returns>A string</returns>
        //string IExportableCsv.Export(bool excludeHeaders)
        //{
        //    // call shared logic (async=false)
        //    return ExportCsvLogic(excludeHeaders, async: false).GetAwaiter().GetResult();
        //}

        /// <summary>
        /// Asynchronous CSV Export
        /// </summary>
        /// <param name="excludeHeaders">Optional without headers</param>
        /// <returns>A string</returns>
        //public async Task<string> ExportAsync(bool excludeHeaders)
        //{
        //    return await ExportCsvLogic(excludeHeaders, async: true);
        //}

        //// Shared logic for both synchronous and async method
        //private async Task<string> ExportCsvLogic(bool excludeHeaders, bool async)
        //{
        //    var creator = new ExportFactory.Services.CsvFileCreator();
        //    creator.SetColumnHeaders(Headers);

        //    // headers?
        //    if (!excludeHeaders)
        //        creator.AddRow(Headers);

        //    // there is only on row 
        //    creator.AddRow(RowData);


        //    //var stringBuilder = creator.GetCsvStringBuilder();
        //    string result = creator.GetCsvStringWriter().ToString();

        //    if (async)
        //    {
        //        await ExportHelper.WriteToStreamAsync(result);
        //    }
        //    else
        //    {
        //        ExportHelper.WriteToStream(result);
        //    }

        //    // Return the generated CSV content as a string
        //    return result;

        //}



        [TableColumn("Eurocode")]
        public NationaleBijlageEnum? NationaleBijlage { get; set; } = NationaleBijlageEnum.NL;

        //[TableColumn("")]
        public string Flag
        {
            get
            {
                if (NationaleBijlage == null)
                    return "";
                else
                {
                    switch (NationaleBijlage)
                    {
                        default:
                        case NationaleBijlageEnum.EU: return Flags.EU;
                        case NationaleBijlageEnum.NL: return Flags.NL;
                        case NationaleBijlageEnum.BE: return Flags.BE;
                        case NationaleBijlageEnum.DE: return Flags.DE;

                    }
                }
            }
        }



        [TableColumn("Ontwerp Levensduur", order: 0)]
        public OntwerpLevensduurEnum? OntwerpLevensduur { get; set; } = OntwerpLevensduurEnum.Vijftig;


        /// <summary>
        /// B.3.1 Gevolgklassen
        /// (1) Ten behoeve van de betrouwbaarheidsdifferentiatie, mogen gevolgklassen (CC), zoals gegeven in tabel B1, 
        /// worden gedefinieerd door het beschouwen van de gevolgen van bezwijken of het slecht functioneren van de
        /// constructie
        /// </summary>
        /// 
        [TableColumn("Gevolgklasse (Consequence Class)", order: 1, width: 4)]
        public GevolgklasseEnum? Gevolgklasse { get; set; } = GevolgklasseEnum.CC2;




        [TableColumn("Betrouwbaarheidsklasse (Reliability Class)", order: 2, width: 5)]
        public BetrouwbaarheidsklasseEnum Betrouwbaarheidsklasse
        {
            get { return this.Gevolgklasse.GetBetrouwbaarheidsklasse(); }
        }


        /// <summary>
        /// B.3.3
        /// Vermenigvuldigingsfactor KFI die wordt toegepast op de partiele factoren.
        /// </summary>
        [TableColumn("K~FI~", stringFormat: "0.0", order: 3, width: 1.5)]
        public double Kfi
        {
            get { return this.Betrouwbaarheidsklasse.GetKfi(); }
        }

        /// <summary>
        /// ξ (xi) is een reductiefactor voor ongunstige, blijvende belastingen G
        /// Deze wordt gebruikt in de fundamentele combinatie (6.10b) en is afhankelijk van de nationale bijlage.
        /// </summary>
        [TableColumn("|xi|", order: 4, stringFormat: "0.00", width: 1.5)]
        public double Xi
        {
            get { return this.NationaleBijlage.GetReductieFactorVoorOngunstigeBlijvendeBelastingen(); }
        }

        /// <summary>
        /// Obsolete, use Custom Attributes 'TableColumn' for export
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>()
        {
            { "NationaleBijlage", "Eurocode [NB]"},
            { "Gevolgklasse", "Gevolgklasse [CC]" },
            { "OntwerpLevensduur", "Levensduur [jaren]"},
            { "Betrouwbaarheidsklasse", "Reliability Class RC" },
            { "Kfi", "K_FI~" },
            { "Xi", "ξ" },
        };


        /// <summary>
        /// Obsolete, generic DataTable with Custom Attributes.
        /// </summary>
        public Dictionary<string, string> RowData { get { return this.GetRowData(); } }


        public void AddToSection(Section section)
        {
            //  voeg een tabel toe aan een secties.

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


        //MigraDoc IExportableMigraDoc.Table


        //string IExportableRtf.Export()
        //{


        //    throw new NotImplementedException();
        //}

        //public Task<string> ExportAsync()
        //{
        //    throw new NotImplementedException();
        //}

        //public Table ExportTable()
        //{
        //    Table table = new Table();

        //    table.AddColumn(Unit.FromMillimeter(30));
        //    table.AddColumn(Unit.FromMillimeter(100));

        //    var row = table.AddRow();
        //    for (int i = 0; i < table.Columns.Count; i++)
        //    {
        //        row.Cells[i].AddParagraph($"cells[{i}]");
        //    }


        //    return table;
        //    throw new NotImplementedException();
        //}

        //public Task<Table> ExportTableAsync()
        //{
        //    throw new NotImplementedException();
        //}
    }
}
