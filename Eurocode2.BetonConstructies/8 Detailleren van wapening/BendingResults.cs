using CommonLibrary;
using ExportFactory.Extensions;
using ExportFactory.Shared;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace Eurocode.BetonConstructies
{



    public class BendingResults : BaseEurocodeContext
    {


        private double _asApplied;
        private BerekeningTypeEnum? _berekeningType = BerekeningTypeEnum.ControleerWapening;
        private Schematisering.ConstructiefModelEnum? _constructiefModel = Schematisering.ConstructiefModelEnum.Balk;
        private double _breedte = 300;
        private double _hoogte = 400;
        private double _moment = 80.80;
        private Snedekrachten? _snedekrachten;

        public override string ToString()
        {




            var result = "";
            result += $"M~Ed~ = {Moment: 0.#} kNm, ";
            result += $"afm. {Breedte}×{Hoogte}/{D} mm, ";
            result += "\r\n";
            result += $"A~s,ben~ = {AsRequired: 0} mm², ";



            if (BerekeningType == BerekeningTypeEnum.ControleerWapening)
            {
                result += $"A~s,toe~ = {AsApplied: 0} mm², ";

                result += $"(UC = {(AsRequired / AsApplied):0.00}), ";

            }

            if (MinimaleWapeningToegepast) result += $"minimale wapening van toepassing, ";



            return result.TrimEnd(',', ' ');
        }

        public BendingResults()
        {
            Beton = new();
            Wapening = new();
            Wapening.PropertyChanged += OnWapeningChanged;
            BerekenEnValideer();
        }


        public BendingResults(BetonContext beton, ParametrischeProfielen.ParametrischProfielContext profiel, WapeningContext wapening, Snedekrachten snedekrachten)
        {
            Beton = beton;
            Profiel = profiel;
            Snedekrachten = snedekrachten;
            //ZRef = zRef;
            Wapening = wapening;
            BerekenEnValideer();
        }



        private void OnWapeningChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Roep de berekening aan bij wijzigingen binnen de WapeningContext
            if (e.PropertyName == nameof(WapeningContext.Tekst))
            {
                BerekenEnValideer();
            }
            if (e.PropertyName == nameof(WapeningContext.Dekking))
            {
                // ? 

                BerekenEnValideer();
            }



            Console.WriteLine($"Wapening (context) gewijzigd: {e.PropertyName}");
            // Hier kun je aanvullende acties uitvoeren, zoals andere properties bijwerken.
        }



        public BerekeningTypeEnum? BerekeningType
        {
            get => _berekeningType;
            set
            {
                if (_berekeningType != value)
                {
                    _berekeningType = value;
                    BerekenEnValideer();
                }
            }

        }

        public Schematisering.ConstructiefModelEnum? ConstructiefModel
        {
            get => _constructiefModel;
            set
            {
                if (_constructiefModel != value)
                {
                    _constructiefModel = value;
                    BerekenEnValideer();
                }
            }
        }


        public BetonContext Beton { get; set; }
        public ParametrischeProfielen.ParametrischProfielContext? Profiel { get; set; } // als geen profiel, dan rechthoek BxH
        public Snedekrachten? Snedekrachten
        {
            get => _snedekrachten;
            set
            {
                if (_snedekrachten != value)
                {
                    _snedekrachten = value;
                    OnPropertyChanged(nameof(Snedekrachten));

                    // Automatisch Moment bijwerken als Snedekrachten verandert
                    if (_snedekrachten != null)
                    {
                        Moment = _snedekrachten.My.Ed;
                    }
                }
            }
        } // als er geen snedekrachten opgegeven dan Moment opgave.


        [TableColumn("positie", order: 0)]
        public string Name { get; set; } = "Schil";


        [TableColumn("M~Ed~ [kNm]", StringFormat = "0.0", Order = 1, Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center)]
        public double Moment
        {
            get => Snedekrachten != null ? Snedekrachten.My.Ed : _moment;
            set
            {
                if (Snedekrachten != null)
                {
                    // Als Snedekrachten niet null is, zet _moment gelijk aan Snedekrachten.My
                    _moment = Snedekrachten.My.Ed;
                }
                else
                {
                    // Als Snedekrachten null is, gebruik de gegeven waarde
                    _moment = value;
                }
            }
        }


        [TableColumn("b [mm]", order: 2, StringFormat = "0", Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center)]
        public double Breedte
        {
            get => Profiel != null ? Profiel.Breedte : _breedte;
            set => _breedte = value;
        }

        [TableColumn("h [mm]", order: 3, StringFormat = "0", Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center)]
        public double Hoogte
        {
            get => Profiel != null ? Profiel.Hoogte : _hoogte;
            set => _hoogte = value;
        }

        public double ZRef
        {
            get
            {
                if (Wapening == null) return 50;
                else
                {

                    return Wapening.ZRef;
                }
            }
        }
        [TableColumn("d [mm]", order: 4, StringFormat = "0.#", Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center)]
        public double D { get { return Hoogte - ZRef; } }


        [TableColumn("x~u~ [mm]", order: 5, StringFormat = "0.#", Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center)]
        public double Xu
        {
            get
            {
                return (D - Math.Pow(D * D - 4.0 * Beton.GetBeta() * Math.Abs(Moment) * 1000000.0 / (Beton.GetAlpha() * Breedte * Beton.Fcd), 0.5)) / (2.0 * Beton.GetBeta());
            }
        }

        public double XuMax
        {
            get
            {
                double betonDsnOpp = Breedte * Hoogte;
                return BuigingContext.GetMaximaleHoogteDrukzoneZonderVoorspanning(Beton, D, betonDsnOpp);
            }
        }

        public double XuDMax
        {
            get
            {
                return XuMax / D;
            }
        }


        public double XuD
        {
            get { return Xu / D; }
        }

        [TableColumn("z [mm]", order: 21, StringFormat = "0.#", Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center)]
        public double Z
        {
            get
            {
                if (IsGedrongenLigger)
                {
                    return Schematisering.GetGedrongenZ(LengteMaatBijGedrongenLiggerInMM, Hoogte, Gedrongen ?? Schematisering.GedrongenEnum.Uitkraging);
                }
                else
                {
                    return D - Beton.GetBeta() * Xu;
                }
            }
        }

        public bool IsGedrongenLigger { get; set; } = false;

        public string LengteMaatBijGedrongenLiggerSymbool
        {
            get
            {
                switch (Gedrongen)
                {
                    default:
                    case null:
                    case Schematisering.GedrongenEnum.Uitkraging:
                        return "a";
                    case Schematisering.GedrongenEnum.StatischBepaald:
                        return "l";
                    case Schematisering.GedrongenEnum.StatischOnbepaald:
                        return "l~0~";

                }
            }
        }

        /// <summary>
        /// De overspanning van de ligger.
        /// In millimeters!
        /// Dit afstand tussen momenten-nulpunten wordt hiermee bedoeld.
        /// </summary>
        public double LengteMaatBijGedrongenLiggerInMM { get; set; } = 400;
        public bool Uitkraging = false;


        public bool StatischBepaald { get; set; } = true;




        public Schematisering.GedrongenEnum? Gedrongen
        {
            get; set;
        }



        [TableColumn("A~s,ben~ [mm²]", Order = 40, StringFormat = "0", Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center)]
        public double AsRequired
        {
            get
            {
                var asBenodigdZuivereBuiging = Math.Max(AsMin, AsBerekend);
                if (IsGedrongenLigger)
                {
                    var asBenodigdGedrongen = Moment * 1e6 / Z / Beton.BetonStaal.Fyd;
                    if (asBenodigdZuivereBuiging > asBenodigdGedrongen)
                    {
                        // melding 
                        // OPMERKING Bij relatief slanke constructies en/ of bij de toepassing van grote hoeveelheden wapening is het
                        // mogelijk dat bij de uitgangspunten geformuleerd in (1)P een lagere waarde van de momentweerstand wordt
                        // gevonden.Deze lagere waarde is dan bepalend voor de momentweerstand van de beschouwde constructie.

                        // (1)P maatgevend
                    }
                    return Math.Max(asBenodigdGedrongen, asBenodigdZuivereBuiging);
                }

                return asBenodigdZuivereBuiging;
            }
        }

        [TableColumn("A~s,toe~ [mm²]", Order = 41, StringFormat = "0", Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center)]
        public double AsApplied
        {
            get => _asApplied;
            set
            {
                if (_asApplied != value)
                {
                    _asApplied = value;
                    BerekenEnValideer();
                }
            }



        }



        public void VerwerkAsApplied()
        {
            _asApplied = BerekeningType switch
            {
                BerekeningTypeEnum.ControleerWapening => Wapening.As,
                _ => Math.Ceiling(AsRequired),
            };



        }

        private WapeningContext _wapening = new WapeningContext();
        public WapeningContext Wapening
        {

            get => _wapening;
            set
            {
                if (_wapening != value)
                {
                    // Koppel oude event los
                    if (_wapening != null)
                    {
                        _wapening.PropertyChanged -= OnWapeningChanged;
                    }

                    _wapening = value;

                    // Koppel nieuwe event
                    if (_wapening != null)
                    {
                        _wapening.PropertyChanged += OnWapeningChanged;
                    }

                    BerekenEnValideer();
                }
            }
        }






        public double SigmaS
        {
            get
            {
                return Ns * 1000 / AsApplied;
            }
        }

        public double Ns
        {
            get
            {
                return Math.Abs(Moment) / (Z / 1000);
            }
        }



        public double AsMin1
        {
            get { return Beton.GetAlpha() * Breedte * XeMin * Beton.Fcd / Beton.BetonStaal.Fyd; }
        }
        public double AsMin2
        {
            get
            {
                return 1.25 * AsBerekend;
            }
        }
        public double AsMin
        {
            get
            {
                return Math.Min(AsMin1, AsMin2);
            }
        }

        public double AsMax
        {
            get
            {
                return Schematisering.GetAsMax(ConstructiefModel, this.Profiel?.Area ?? (this.Breedte * this.Hoogte));
                //return 0.04 * this.Profiel?.Area ?? (this.B * this.H);
                //return ConstructiefModel?.GetAsMax(this.Profiel?.Area ?? (this.B * this.H));
            }
        }


        public double AsBerekend
        {
            get
            {
                return Beton.GetAlpha() * Breedte * Xu * Beton.Fcd / Beton.BetonStaal.Fyd;
            }
        }

        public double Iy
        {
            get
            {
                return Breedte * Hoogte * Hoogte / 6.0;
            }
        }

        public double MeMin { get { return Beton.Fctm * Iy / 1000 / 1000; } }
        public double XeMin { get { return (D - Math.Pow(D * D - 4.0 * Beton.GetBeta() * MeMin * 1000000.0 / (Beton.GetAlpha() * Breedte * Beton.Fcd), 0.5)) / (2.0 * Beton.GetBeta()); } }





        public bool MinimaleWapeningToegepast
        {
            get
            {
                return AsMin > AsBerekend;
            }
        }

        public override bool IsAkkoord()
        {
            return Valideer();
        }

        protected override void Bereken()
        {
            Wapening.SetZRef();
            VerwerkAsApplied();
            // volgens mij gaat dit volledig automatisch...
        }

        protected override bool Valideer()
        {
            if (Xu > XuMax)
            {
                AddMeldingWaarschuwing("hoogte drukzone niet akkoord");
                //Meldingen.Add(new(MeldingType.Waarschuwing, "overschrijding maximale hoogte drukzone"));
                return false;
            }
            if (AsApplied < AsRequired)
            {
                AddMeldingWaarschuwing("onvoldoende wapening");
                //Meldingen.Add(new(MeldingType.Waarschuwing, "overschrijding maximale hoogte drukzone"));
                return false;
            }


            if (AsApplied > this.AsMax)
            {
                AddMeldingWaarschuwing("overschrijding maximale wapening (voor balk)");
                return false;
            }


            return true;

        }

        public override MarkupString ToHtml(bool isDraaiTabel = true)
        {
            try
            {
                return this.ToHtmlTable(isDraaiTabel);
            }
            catch (Exception ex)
            {
                // Log de fout of geef een melding weer
                Console.WriteLine($"Fout bij het genereren van HTML: {ex.Message}");
                return new MarkupString("Fout bij het genereren van HTML.");
            }

            throw new NotImplementedException();
        }
    }




}
