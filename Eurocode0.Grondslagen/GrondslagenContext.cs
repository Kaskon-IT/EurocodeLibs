
//using CsvFactory;
//using CsvFactory.Interfaces;

///using CsvFactory;
using CommonLibrary;
using CommonLibrary.Interfaces;
using ExportFactory.MigraDocContentModels;
using Microsoft.AspNetCore.Components;
using MigraDoc.DocumentObjectModel;
using System.ComponentModel;


namespace Eurocode.Grondslagen
{
    public class GrondslagenContext : BaseEurocodeContext, IMarkupConvertible, INotifyPropertyChanged
    {
        //public event PropertyChangedEventHandler? PropertyChanged;

        //protected void OnPropertyChanged(string propertyName)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}

        // check of alles nog werkt, met bovenstaande uitgecommentarieerde code


        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(bool alleenFactoren)
        {
            if (alleenFactoren)
                return $"{Betrouwbaarheidsklasse} → K<sub>FI</sub> = {Kfi:0.##}, {NationaleBijlage} → ξ = {Xi:0.##}";

            else
                return $"{Gevolgklasse} → {Betrouwbaarheidsklasse}, {OntwerpLevensduur.GetOntwerplevensduurTekst()}, norm: {NationaleBijlage} → ξ = {Xi:0.##}";
        }


        public override string Heading { get; set; } = "Grondslagen";

        //[TableColumn("Eurocode")]
        public NationaleBijlageEnum? NationaleBijlage { get; set; } = NationaleBijlageEnum.NL;

        //[TableColumn("NB")]
        public string FlagSvg
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

        //[TableColumn("Land")]
        public string FlagEmoji
        {
            get
            {
                switch (NationaleBijlage)
                {
                    default:
                    case NationaleBijlageEnum.NL: return "🇳🇱";
                    case NationaleBijlageEnum.EU: return "🇪🇺";
                    case NationaleBijlageEnum.BE: return "🇧🇪";
                    case NationaleBijlageEnum.DE: return "🇩🇪";
                }
            }
        }


        public string NormPrefix
        {
            get
            {
                switch (NationaleBijlage)
                {
                    default:
                    case NationaleBijlageEnum.EU: return "CEN";
                    case NationaleBijlageEnum.NL: return "NEN";
                    case NationaleBijlageEnum.BE: return "NBN";
                    case NationaleBijlageEnum.DE: return "DIN";
                }
            }
        }
        public string NormTitel => $"{NormPrefix}-EN 1990 Grondslagen voor het ontwerp van constructies";



        [TableColumn(Label = "ontwerplevensduur", Article = "2.3")]
        public OntwerpLevensduurEnum? OntwerpLevensduur { get; set; } = OntwerpLevensduurEnum.Vijftig;


        /// <summary>
        /// B.3.1 Gevolgklassen
        /// (1) Ten behoeve van de betrouwbaarheidsdifferentiatie, mogen gevolgklassen (CC), zoals gegeven in tabel B1, 
        /// worden gedefinieerd door het beschouwen van de gevolgen van bezwijken of het slecht functioneren van de
        /// constructie
        /// </summary>
        /// 
        [TableColumn(Label = "gevolgklasse", Article = "Bijlage B")]
        public GevolgklasseEnum? Gevolgklasse
        {
            get => _gevolgklasse;
            set
            {
                if (_gevolgklasse != value)
                {
                    _gevolgklasse = value;
                    OnPropertyChanged(nameof(Gevolgklasse));
                }
            }
        }
        private GevolgklasseEnum? _gevolgklasse = GevolgklasseEnum.CC2;



        [TableColumn(Label = "betrouwbaarheidsklasse", Article = "Bijlage B")]
        public BetrouwbaarheidsklasseEnum Betrouwbaarheidsklasse
        {
            get { return this.Gevolgklasse.GetBetrouwbaarheidsklasse(); }
        }


        /// <summary>
        /// B.3.3
        /// Vermenigvuldigingsfactor KFI die wordt toegepast op de partiele factoren.
        /// </summary>
        [TableColumn(
            Symbol = "<i>K</i><sub>FI</sub>",
            StringFormat = "0.##",
            Article = "Bijlage B",
            Label = "betrouwbaarheidsdifferentiatie",
            Description = "factor toepasbaar op belastingen ten behoeve van de betrouwbaarheidsdifferentiatie")]
        public double Kfi
        {
            get { return this.Betrouwbaarheidsklasse.GetKfi(); }
        }

        /// <summary>
        /// ξ (xi) is een reductiefactor voor ongunstige, blijvende belastingen G
        /// Deze wordt gebruikt in de fundamentele combinatie (6.10b) en is afhankelijk van de nationale bijlage.
        /// </summary>
        [TableColumn(
            Symbol = "ξ",
            StringFormat = "0.##",
            Label = "reductiefactor",
            Article = "Tabel NB.4",
            Description = "reductiefactor voor ongunstige blijvende belastingen (nationale bijlage)")]
        public double Xi
        {
            get { return this.NationaleBijlage.GetReductieFactorVoorOngunstigeBlijvendeBelastingen(); }
        }



        public void AddToSection(Section section)
        {
            //  voeg een tabel toe aan een secties.

            throw new NotImplementedException();
        }


        //public MarkupString ToMarkupString()
        //{
        //    return ToMarkupString(false);
        //}

        public new MarkupString ToMarkupString(bool alleenFactoren)
        {
            return CommonLibrary.Helpers.MarkupHelper.ToMarkupString(this.ToString(alleenFactoren));
        }



        protected override void Bereken()
        {
            // kan niet berekend worden
        }

        protected override bool Valideer()
        {
            Meldingen.Clear();
            if (NationaleBijlage == null)
            {
                Meldingen.Add(new Melding(MeldingType.Error, "Nationale bijlage is niet opgegeven"));
                return false;
            }


            return true;
        }

    }
}
