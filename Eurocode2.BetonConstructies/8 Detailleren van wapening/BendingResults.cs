using CommonLibrary;
using CommonLibrary.Helpers;
using Profielen.Parametrisch;
using Profielen.Beton;
using Eurocode.Belastingen;
using ExportFactory.Shared;
using System.ComponentModel;
using K = CommonLibrary.EurocodeKeys;

namespace Eurocode.BetonConstructies
{

    //public class TorsionResult : BaseEurocodeContext
    //{
    //    public override string Heading { get; set; } = "Torsiewapening";
    //    public double TEd { get; set; } = 0;
    //    public required ParametrischProfielContext Profile { get; set; }
    //    public 
        
    //}



    public class BendingResults : BaseEurocodeContext
    {

        public override string Heading { get; set; } = "Momentwapening";

        private double _asApplied;
        private BerekeningTypeEnum? _berekeningType = BerekeningTypeEnum.ControleerWapening;
        private Schematisering.ConstructiefModelEnum? _constructiefModel = Schematisering.ConstructiefModelEnum.Balk;
        private double _breedte = 300;
        private double _hoogte = 400;
        private double _moment = 0;
        private SectionForces? _snedekrachten;
        private VerankeringLangswapeningContext? _verankeringsLengte;

        public string MeldingTeksten
        {
            get
            {
                return string.Join("; ", Meldingen.Select(m => m.ToMarkDownString()));
            }
        }


        public override string ToString()
        {
            var result = "";
            result += $"M~Ed~ = {Moment: 0.#} kNm, ";
            result += $"afm. {Breedte}×{Hoogte}/{D} mm, ";
            result += "\r\n";
            result += $"A~s,req~ = {AsRequired: 0} mm², ";



            if (BerekeningType == BerekeningTypeEnum.ControleerWapening)
            {
                result += $"A~s,prov~ = {AsApplied: 0} mm², ";
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
            _verankeringsLengte = new VerankeringLangswapeningContext() { Beton = Beton };
            BerekenEnValideer();
        }


        public BendingResults(BetonContext beton, BetonProfiel profiel, WapeningContext wapening, SectionForces snedekrachten)
        {
            Beton = beton;
            Profiel = profiel;
            Snedekrachten = snedekrachten;
            //ZRef = zRef;
            Wapening = wapening;
            _verankeringsLengte = new VerankeringLangswapeningContext()
            {
                Beton = Beton,
                StaafType = VerankeringLangswapeningContext.StaafTypeEnum.Trekstaaf,
                GoedeAanhechtingOmstandigheden = true,
                Diameter = wapening.GemiddeldeDiameter
            };
            BerekenEnValideer();
        }



        private void OnWapeningChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Roep de berekening aan bij wijzigingen binnen de WapeningContext
            if (e.PropertyName == nameof(WapeningContext.Tekst))
            {
                BerekenEnValideer();
            }
            if (e.PropertyName == nameof(WapeningContext.ReferentieDekking))
            {
                BerekenEnValideer();
            }
            // Console.WriteLine verwijderd - debug alleen als nodig
        }




        public VerankeringLangswapeningContext? VerankeringsLengte
        {
            get => _verankeringsLengte;
            set
            {
                if (_verankeringsLengte != value)
                {
                    _verankeringsLengte = value;
                    OnPropertyChanged(nameof(VerankeringsLengte));
                }
            }
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


        private BetonProfiel? _profiel;
        public BetonProfiel? Profiel
        {
            get => _profiel;
            set
            {
                if (_profiel != null)
                    _profiel.PropertyChanged -= Profiel_PropertyChanged;

                _profiel = value;

                if (_profiel != null)
                    _profiel.PropertyChanged += Profiel_PropertyChanged;

                // Laat OnPropertyChanged/OnUpdated weten dat Profiel is veranderd
                OnPropertyChanged(nameof(Profiel));
            }
        }

        private void Profiel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Profiel.Hoogte))
            {
                // Trigger recalculatie automatisch (hier kom je terecht als je het Profiel.Hoogte aanpast)
                // dus buiten deze class om. Dit is de enige juiste methode.
                BerekenEnValideer();
                // Event ook melden voor bindingen
                OnPropertyChanged(nameof(Hoogte));
            }
            if (e.PropertyName == nameof(Profiel.Breedte))
            {
                BerekenEnValideer();
                OnPropertyChanged(nameof(Profiel.Breedte));
            }
        }

        private void Snedekrachten_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Snedekrachten.My))
            {
                BerekenEnValideer();
                OnPropertyChanged(nameof(Moment));
            }
        }


        public SectionForces? Snedekrachten
        {
            get => _snedekrachten;
            set => SetNestedProperty(ref _snedekrachten, value);
            //{




            //    _snedekrachten = value;
            //    SubscribeNestedProperty(_snedekrachten); // event subscriben
            //    Moment = _snedekrachten?.My.Ed ?? 0;
            //    OnPropertyChanged(nameof(Snedekrachten));

            //    //ReplaceNestedProperty(ref _snedekrachten, value);
            //    //Moment = _snedekrachten?.My.Ed ?? 0;

            //    //if (_snedekrachten != value)
            //    //{
            //    //    _snedekrachten = value;
            //    //    OnPropertyChanged(nameof(Snedekrachten));

            //    //    // Automatisch Moment bijwerken als Snedekrachten verandert
            //    //    if (_snedekrachten != null)
            //    //    {
            //    //        Moment = _snedekrachten.My.Ed;
            //    //    }
            //    //}
            //}
        } // als er geen snedekrachten opgegeven dan Moment opgave.


        [TableColumn(Label = "pos", Unit ="m", Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Left, Width = 1.5)]
        public string PosLabel { get; set; } = "midden";
        public bool PosLabelVisible { get; set; } = true;

        [TableColumn(Label = "zijde", Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Left, Width = 2.0)]
        public string PosWapBovenOnder
        {
            get
            {
                if (Moment == 0) return "-";
                else if (Moment > 0) return "◠ boven";
                else return "◡ onder";
            }
        }


        public string Name { get; set; } = "-";

        //[TableColumn(Label = "DEBUG", Symbol = "Beta")]
        [TableColumn(Symbol = "*β*", Description = "vormfactor" ,Visible = false, Unit = "-", StringFormat = "0.###" )]
        public double Beta => Beton.GetBeta();

        [TableColumn(Symbol = "*α*", Description = "vormfactor", Visible = false, Unit = "-", StringFormat = "0.###")]
        public double Alpha => Beton.GetAlpha();



        [TableColumn(Symbol = "<i>M<sub>y,Ed</sub></i>", Label = "moment rekenwaarde", Unit = "kNm",
            Width = 1.5,
            Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Left)]
        public double Moment
        {
            get
            {
                if (Snedekrachten != null && Snedekrachten.My != _moment)
                {
                    _moment = Snedekrachten.My;
                    BerekenEnValideer();
                }
                return Snedekrachten != null ? Snedekrachten.My : _moment;

            }
            internal set
            {
                double newValue;

                if (Snedekrachten != null)
                {
                    // Als Snedekrachten is ingesteld, override met de waarde uit Snedekrachten
                    newValue = Snedekrachten.My;
                }
                else
                {
                    // Anders gebruik de opgegeven waarde
                    newValue = value;
                }

                if (!newValue.Equals(_moment)) // alleen updaten bij verandering
                {
                    _moment = newValue;
                    BerekenEnValideer();
                    OnPropertyChanged(nameof(Moment));
                }
            }
        }


        [TableColumn(Visible = false, Symbol = "*b*", Description = "breedte", Unit = "mm")]
        public double Breedte
        {
            get => Profiel != null ? Profiel.Breedte : _breedte;
            internal set
            {
                _breedte = value;
                BerekenEnValideer();
            }
        }

        //[TableColumn(Visible = false, Symbol = "<i>b x h/d</i>",Label = "<i>b × h/d</i>", Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Left, Width = 2.5)]
        public string Afmeting => $"{Breedte:0} × {Hoogte:0}/{D:0.#}";


        [TableColumn(Visible = false, Symbol = "*h*", Description = "hoogte", Unit = "mm")] 
        public double Hoogte
        {
            get => Profiel != null ? Profiel.Hoogte : _hoogte;
            internal set
            {
                _hoogte = value;
                BerekenEnValideer();
                OnPropertyChanged(nameof(Hoogte));
            }

        }


        public double ZRefZonderDekking { get; set; } = 50;


        public double ZRef
        {
            get
            {
                if (Wapening == null)
                    return ZRefZonderDekking;

                return Wapening.ReferentieAfstand;

            }
        }

        //[TableColumn(Symbol = "<i>d</i>", Label = "nuttige hoogte", Unit = "mm",
        //    Width = 1.5,
        //    Key = K.NuttigeHoogte,
        //    StringFormat = "0.#", Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Left)]
        [TableColumn(
            Symbol = "<i>d</i>", 
            Label = "nuttige hoogte", 
            Unit = "mm",
            StringFormat = "0.#",
            Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Left, 
            Width = 1.5)]
        public double D { get { return Hoogte - ZRef; } }


        [TableColumn(Symbol = "*x<sub>u</sub>*", Label = "hoogte drukzone", Unit = "mm",
            StringFormat = "0.#",
            Width = 1.0,
            Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Left)]
        public double Xu
        {
            get
            {
                return (D - Math.Pow(D * D - 4.0 * Beton.GetBeta() * Math.Abs(Moment) * 1000000.0 / (Beton.GetAlpha() * Breedte * Beton.Fcd), 0.5)) / (2.0 * Beton.GetBeta());
            }
        }
        public Formula XuFormula => new()
        {
            StaticValue = @"x_{u} = \frac{d - \sqrt{d^2 - 4 \cdot \beta \cdot M_{Ed} \cdot 10^6 / (\alpha \cdot b \cdot f_{cd})}}{2 \cdot \beta}",
            DynamicValue = $"= ({D:0.#} - sqrt({D:0.#}^2 - 4 * {Beta:0.###} * {Math.Abs(Moment):0.#} * 10^6 / ({Alpha:0.###} * {Breedte:0.#} * {Beton.Fcd:0.#})))/(2 * {Beta:0.###}) = {Xu:0.#} mm"
        };

        [TableColumn(
            Visible = false,
            Symbol = "*x<sub>u,max</sub>*", 
            Label = "maximale hoogte drukzone", Unit = "mm",
           StringFormat = "0.#",
           Width = 1.0,
           Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Left)]
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

        [TableColumn(
            Visible = false,
            Symbol = "*U.C.*",
            Label = "unity check", Unit = "-",
           StringFormat = "0.00",
           Width = 1.0,
           Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Left)]
        public double UnityCheck
        {
            get
            {
                return _unityChecks.Max();
            }
        }


        private List<double> _unityChecks => new List<double> { XuD / XuDMax, AsRequired / Wapening.As, AsApplied / AsMax };



        [TableColumn(
            Visible = !false,
            Symbol = "*x<sub>u</sub> / d*",
            Label = "verhouding drukzone / nuttige hoogte", Unit = "-",
           StringFormat = "0.##",
           Width = 1.0,
           Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Left)]
        public double XuD
        {
            get { return Xu / D; }
        }

        [TableColumn(
            Symbol = "<i>z</i>",
            Unit = "mm", 
            Label = "inwendige hefboomsarm",
            Width = 1.5,
            StringFormat = "0.#", 
            Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Left)]
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
        public Formula ZFormula
        {
            get
            {
                
                if (IsGedrongenLigger)
                {
                    switch (Gedrongen)
                    {
                        case Schematisering.GedrongenEnum.Uitkraging:
                            var dynVal = Z > (0.4 * LengteMaatBijGedrongenLiggerInMM + 0.4 * Hoogte) ?
                                @$"= 1.6 \cdot {LengteMaatBijGedrongenLiggerInMM:0.#}={Z:0.#} mm" :
                                @$"= 0.4 \cdot {LengteMaatBijGedrongenLiggerInMM:0.#} + 0.4 \cdot {Hoogte} = {Z:0.#} mm";
                            return new Formula()
                            {
                                StaticValue = $"z = 0.4a + 0.4h \\leq 1.6a (uitkraging)",
                                DynamicValue = dynVal
                            };
                        case Schematisering.GedrongenEnum.StatischBepaald:
                            var dynValStatischBepaald = Z > (0.2 * LengteMaatBijGedrongenLiggerInMM + 0.4 * Hoogte) ?
                                $@"= 0.6 \cdot l = {Z:0.#} mm" :
                                $@"= 0.2 \cdot {LengteMaatBijGedrongenLiggerInMM} + 0.4 \cdot {Hoogte} = {Z:0.#} mm";
                            return new Formula()
                            {
                                StaticValue = @"z = 0.2l + 0.4h \leq 0.6l",
                                DynamicValue = dynValStatischBepaald
                            };
                        case Schematisering.GedrongenEnum.StatischOnbepaald:
                            var dynValStatischOnbepaald = Z > (0.3 * LengteMaatBijGedrongenLiggerInMM + 0.3 * Hoogte) ?
                                $@"= 0.8 \cdot {LengteMaatBijGedrongenLiggerInMM:0.#} = {Z:0.#} mm" :
                                $@"= 0.3 \cdot {LengteMaatBijGedrongenLiggerInMM:0.#} + 0.3 \cdot {Hoogte:0} = {Z:0.#} mm";
                            
                            return new Formula()
                            {
                                StaticValue = "z = 0.3l_0 + 0.3h \\leq 0.8l_0",
                                DynamicValue = dynValStatischOnbepaald
                            };
                        default:

                            return new Formula();

                    }
                }
                else
                {
                    return new Formula()
                    {
                        StaticValue = @"z = d - β \cdot x_{u}",
                        DynamicValue = $"= {D:0.#} - {Beta:0.###} * {Xu:0.#} = {Z:0.#} mm"
                    };
                }

                
            }
            
        } 
        
            
           

        private bool _isGedrongenLigger;

        public bool IsGedrongenLigger
        {
            get => _isGedrongenLigger;
            set
            {
                if (_isGedrongenLigger != value)
                {
                    _isGedrongenLigger = value;
                    OnPropertyChanged(nameof(IsGedrongenLigger));
                    BerekenEnValideer();
                }
            }
        }


        private bool IsGedrongenMaarSlankIsMaatgevend
        {
            get; set;
        }


        public string LengteMaatBijGedrongenLiggerSymbool
        {
            get
            {
                switch (Gedrongen)
                {
                    default:
                    case null:
                    case Schematisering.GedrongenEnum.Uitkraging:
                        return "<i>a</i>";
                    case Schematisering.GedrongenEnum.StatischBepaald:
                        return "<i>l</i>";
                    case Schematisering.GedrongenEnum.StatischOnbepaald:
                        return "<i>l</i><sub>0</sub>";
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



        [TableColumn(
            Symbol = "*A<sub>s,req</sub>*",
            Unit = "mm²", 
            Label = "benodigde wapening",
            Width = 1.5,
            StringFormat = "0",
            Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Left)]
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
                        AddMelding(StandaardMeldingenCatalogus.GedrongenLiggerNietMaatgevend);
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
        public Formula AsRequiredFormula => new()
        {
            StaticValue = @"A_{s,req} = max(A_{s,min}; A_{s,ber})",
            DynamicValue = $"= max({AsMin:0}; {AsBerekend:0}) = {AsRequired:0}"
        };


        [TableColumn(
            Symbol = "*A<sub>s,prov</sub>*",
            Unit = "mm²",
            Label = "toegepaste wapening",
            Width = 1.5,
            StringFormat = "0",
            Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Left
            )]
        public double AsApplied
        {
            get => _asApplied;
            internal set
            {
                if (_asApplied != value)
                {
                    _asApplied = value;
                    BerekenEnValideer();
                }
            }



        }

        [TableColumn(
            Symbol = "wapening", 
            Label = "wapening",
            Width = 3.0, 
            Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Left )]
        public string AsProvidedText
        {
            get 
            {
                if (Wapening != null)
                {
                    return Wapening.SanitizedTekst();
                }
                else
                {
                    return "?";
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


        [TableColumn(
            Visible = false,
            Label = "minimale wapening", 
            Symbol = "<i>A<sub>s,min</sub></i>", 
            Unit = "mm²", 
            StringFormat = "0")]
        public double AsMin
        {
            get
            {
                return Math.Min(AsMin1, AsMin2);
            }
        }
        public Formula AsMinFormula => new()
        {
            StaticValue = "A_{s,min} = min (A_{s,min1}; A_{s,min2})",
            DynamicValue = $"= min({AsMin1:0};{AsMin2:0}) = {AsMin:0}"
        };

        /// <summary>
        /// Gebruik artikel 7.3.1 minimale wapening voor gecontroleerde scheurbeheersing.
        /// </summary>
        public bool MinimaleWapeningScheurbeheersingToepassen { get; set; } = true;
        public double AsMinScheurbeheersing
        {
            get
            {
                if (MinimaleWapeningScheurbeheersingToepassen)
                {
                    Scheurbeheersing.ScheurwijdteMinimumWapening ScheurwijdteAsMin = new()
                    {
                        Beton = Beton,

                    };
                    return ScheurwijdteAsMin.AsMin;
                }
                else return double.MaxValue; // geen minimale wapening voor scheurbeheersing toepassen

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

        [TableColumn(Visible = false, Symbol = "*A~s,ber~*", Description = "berekende wapening", Unit = "mm²", StringFormat = "0")]
        public double AsBerekend
        {
            get
            {
                return Beton.GetAlpha() * Breedte * Xu * Beton.Fcd / Beton.BetonStaal.Fyd;
            }
        }
        public Formula AsBerekendFormula => new() { StaticValue = @"A_{s,ber} = α \cdot b \cdot x_{u} \cdot f_{cd} / f_{yd}" };


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


        //private List<int> _meldingCodes = [];

        [TableColumn(
            Label = "opm.", 
            Width = 2.0,
            Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Left)]
        public override string MeldingNummers => base.MeldingNummers;

        /// <summary>
        /// Opneembaar moment van de toegepaste trekwapening uit krachtenevenwicht (zonder drukwapening),
        /// met het teken van <see cref="Moment"/>.
        /// </summary>
        public double MRd
        {
            get
            {
                if (Wapening == null) return 0;
                var mRd = RechthoekBuigingCalculator.BerekenMRd(Beton, Breedte, D, AsApplied).MRd;
                return Moment < 0 ? -mRd : mRd;
            }
        }



        protected override void Bereken()
        {
            // als er geen wapening is dan 
            if (string.IsNullOrWhiteSpace(Wapening.Tekst))
            {
                // neem eerst de ondergrens (indien aanwezig) anders waarde r8-150
                Wapening.Tekst = Wapening.TekstOndergrens ?? "8-150";

            }

            Wapening.SetZRef();
            VerwerkAsApplied();

            // als de wapening wijzigt, dan ook
            if (_verankeringsLengte != null)
            {
                _verankeringsLengte.Diameter = Wapening.GemiddeldeDiameter;
            }

            // controleer of slank maatgevend 
            if (_isGedrongenLigger)
            {
                if (AsRequired == AsBerekend)
                {

                }
            }

            // volgens mij gaat dit volledig automatisch...
        }

        protected override bool Valideer()
        {
            //_meldingCodes?.Clear();
            bool returnVal = true;

            if (double.IsNaN(AsRequired))
            {
                AddMelding(StandaardMeldingenCatalogus.BerekeningNietAkkoord);
                returnVal = false;
            }

            if (double.IsNaN(Xu))
            {
                AddMelding(StandaardMeldingenCatalogus.BerekeningNietAkkoord);
                returnVal = false;
            }

            if (Xu > XuMax)
            {
                AddMelding(StandaardMeldingenCatalogus.OverschrijdingDrukzone);
                returnVal = false;
            }
            if (AsApplied < AsRequired)
            {
                AddMelding(StandaardMeldingenCatalogus.OnvoldoendeLangsWapening);
                returnVal = false;
            }


            if (AsApplied > this.AsMax)
            {
                AddMelding(StandaardMeldingenCatalogus.OverschrijdingMaximaleWapening);
                returnVal = false;
            }


            // OPMERKINGEN (do not return false)
            if (MinimaleWapeningToegepast)
            {
                AddMelding(StandaardMeldingenCatalogus.MinimaleWapening);
            }

            if (IsGedrongenLigger)
                AddMelding(StandaardMeldingenCatalogus.GedrongenLigger);


            return returnVal;

        }


    }




}
