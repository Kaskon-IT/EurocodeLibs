using CommonLibrary;
using CommonLibrary.Extensions;
using CommonLibrary.Models;
using ExportFactory.Extensions;
using ExportFactory.Shared;
using Microsoft.AspNetCore.Builder;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Eurocode.BetonConstructies
{
    /// <summary>
    /// 3 Materialen
    /// 3.1 Beton en 3.2 Betonstaal
    /// Verzameling gegevens voor berekening van betonconstructies.
    /// </summary>
    public partial class BetonContext : BaseMateriaal
    {
        public override MateriaalType Type => MateriaalType.Beton;
        public override double SoortelijkGewicht => 2500; // kg/m³
        public override string UserFriendlyName => $"{BetonSterkteKlasseGebruiksvriendelijkeNaam}";
        public override string Naam => UserFriendlyName;
        public override string Eurocode => "EC2 - Betonconstructies";
        public override double E => Ecm;
        public override double G => E / (2 * (1 + PoissonFactor));

        public BetonContext(int fck)
        {

            if (fck >= 90) Betonsterkteklasse = BetonsterkteklasseEnum.C90_105;
            else if (fck >= 80) Betonsterkteklasse = BetonsterkteklasseEnum.C80_95;
            else if (fck >= 70) Betonsterkteklasse = BetonsterkteklasseEnum.C70_85;
            else if (fck >= 60) Betonsterkteklasse = BetonsterkteklasseEnum.C60_75;
            
            else if (fck >= 55) Betonsterkteklasse = BetonsterkteklasseEnum.C55_67;
            else if (fck >= 50) Betonsterkteklasse = BetonsterkteklasseEnum.C50_60;
            else if (fck >= 45) Betonsterkteklasse = BetonsterkteklasseEnum.C45_55;
            else if (fck >= 40) Betonsterkteklasse = BetonsterkteklasseEnum.C40_50;
            else if (fck >= 35) Betonsterkteklasse = BetonsterkteklasseEnum.C35_45;
            else if (fck >= 30) Betonsterkteklasse = BetonsterkteklasseEnum.C30_37;
            else if (fck >= 25) Betonsterkteklasse = BetonsterkteklasseEnum.C25_30;
            else if (fck >= 20) Betonsterkteklasse = BetonsterkteklasseEnum.C20_25;
            
            else if (fck >= 16) Betonsterkteklasse = BetonsterkteklasseEnum.C16_20;
            else Betonsterkteklasse = BetonsterkteklasseEnum.C12_15;
        }

        public BetonContext() 
        {
            PartieleFactor = 1.5;
        }
        /// <summary>
        /// Maakt een kopie.
        /// </summary>
        /// <param name="vorige"></param>
        public BetonContext(BetonContext vorige) 
        {
            Betonsterkteklasse = vorige.Betonsterkteklasse;
            IsOntwerpSituatieBuitenGewoon = vorige.IsOntwerpSituatieBuitenGewoon;
            CementKlasse = vorige.CementKlasse;
            Alpha = vorige.Alpha;
            Beta = vorige.Beta;
            SpanningRekDiagram = vorige.SpanningRekDiagram;
            BetonStaal = new BetonStaalContext(vorige.BetonStaal);
            PartieleFactor = vorige.PartieleFactor;

        }

        public override string Heading { get; set; } = "Beton";

        // 3.2 betonstaal als onderdeel
        /// <summary>
        /// 3.2 Betonstaal is onderdeel van de betoncontext.
        /// </summary>
        public BetonStaalContext BetonStaal { get; set; } = new();


        /// <summary>
        /// 3.1.7 Spanning-rekrelatie voor doorsnedeberekeningen; standaard parabool-rechthoek.
        /// </summary>
        [TableColumn(
            Label = "spanning-rekrelatie",
            Order = 1,
            Description = "Voor het berekenen van dwarsdoorsneden mag een parabool-rechthoekdiagram of bi-lineaire spanning-rekrelatie worden gebruikt",
            Article = "3.1.7")]
        public SpanningRekDiagramType? SpanningRekDiagram
        {
            get => _spanningRekDiagram;
            set => SetProperty(ref _spanningRekDiagram, value);
        }
        public enum SpanningRekDiagramType
        {
            [Description("Parabolisch")]
            Parabolisch,
            [Description("Bi-Lineair")]
            BiLineair
        }


        [TableColumn(Label = "betonrek",
            Symbol = "<i>ε</i><sub>c</sub>",
            Article = "3.1.7",
            Description = "De optredende betonrek"
            )]
        public double RekEpsilonC { get; private set; }


        //[TableColumn(Label = "betonspanning",
        //    Symbol = "<i>σ</i><sub>c</sub>",
        //    Description = "De optredende betonspanning is afhankelijk van de gebruikte spanning-rekrelatie en de optredende rek (<i>ε</i><sub>c</sub>)",
        //    Article = "3.1.7",
        //    Unit = "N/mm²")]
        public double SpanningSigmaC
        {
            get
            {
                return this.GetSigmaCd(RekEpsilonC);
            }
        }
        public Formula SpanningSigmaCFormula
        {
            get
            {
                switch (SpanningRekDiagram)
                {
                    default:
                        return new() { };

                    case SpanningRekDiagramType.Parabolisch:
                        if (RekEpsilonC >= 0 && RekEpsilonC <= EpsilonCu)
                        {
                            return new()
                            {
                                Name = "(3.17)",
                                StaticValue = @$"\sigma_c=f_{{cd}}\left[ 1 - \left( 1 - \frac{{\epsilon_c}}{{\epsilon_{{c2}} \right)^n  \right]",
                                DynamicValue = @$"\sigma_c={Fcd.ToTeX()} \left[ 1 - \left( 1 - \frac{{{RekEpsilonC.ToTeX()}}}{{{EpsilonC2.ToTeX()}}} \right)^{{{FactorN.ToTeX()}}}  \right]"
                            };
                        }
                        else if (RekEpsilonC >= EpsilonCu)
                        {
                            return new()
                            {
                                Name = "(3.18)",
                                StaticValue = @$"\sigma_c=f_{{cd}}",
                                DynamicValue = @$"\sigma_c={Fcd.ToTeX()}"
                            };
                        }
                        else
                        {
                            return new() { Name = "error", StaticValue = @"rek \epsilon_c ligt buiten de grenswaarde van het parabool-rechthoekdiagram onder druk." };
                        }


                    case SpanningRekDiagramType.BiLineair:
                        return new() { Name = "", StaticValue = @"\sigma_c = f_{cd}", DynamicValue = @$"\sigma_c={Fcd.ToTeX()}" };




                }
            }
        }





        /// <summary>
        /// 3.1.2 (6) Het soort cement.
        /// </summary>
        [TableColumn("cementklasse",
            Order = 21,
            Description = "cement klasse",
            Article = "3.1.2 (6)")]
        public CementklasseEnum? CementKlasse
        {
            get => _cementklasse;
            set => SetProperty(ref _cementklasse, value);
        }
        private CementklasseEnum? _cementklasse = CementklasseEnum.N;


        public double CoefficientCementKlasse
        {
            get
            {
                return CementKlasse switch
                {
                    CementklasseEnum.S => 0.38,// slow
                    CementklasseEnum.N => 0.25,// normal
                    CementklasseEnum.R => 0.20,// rapid
                    _ => 0.20,
                };
            }
        }


   



        private BetonsterkteklasseEnum? _betonsterkteklasse = BetonsterkteklasseEnum.C20_25;
        private SpanningRekDiagramType? _spanningRekDiagram = SpanningRekDiagramType.Parabolisch;

        [TableColumn("betonsterkteklasse",
            Article = "3.1.2",
            Description = "Aanduiding sterkteklasse van beton met de letter C{cilinderdruksterkte}/{kubusdruksterkte}"

            )]
        public BetonsterkteklasseEnum? Betonsterkteklasse
        {
            get => _betonsterkteklasse;
            set
            {
                if (_betonsterkteklasse != value)
                {
                    _betonsterkteklasse = value;
                    OnPropertyChanged(nameof(Betonsterkteklasse));
                    BerekenEnValideer();
                }
            }
        }





        public string BetonSterkteKlasseGebruiksvriendelijkeNaam
        {
            get
            {
                return $"C{Fck}/{FckCube}";
            }
        }







        /// <summary>
        /// De representieve cilinder druksterkte in N/mm²
        /// </summary>
        [TableColumn("cilinderdruksterkte",
            Symbol = "<i>f</i><sub>ck</sub>",
            Article = "3.1.2 (3)",
            Description = "is de karakteristieke cilinderdruksterkte van beton na 28 dagen",
            Unit = "N/mm²")]
        public double Fck
        {
            get
            {
                return Betonsterkteklasse switch
                {
                    BetonsterkteklasseEnum.C12_15 => 12,
                    BetonsterkteklasseEnum.C16_20 => 16,
                    BetonsterkteklasseEnum.C20_25 => 20,
                    BetonsterkteklasseEnum.C25_30 => 25,
                    BetonsterkteklasseEnum.C30_37 => 30,
                    BetonsterkteklasseEnum.C35_45 => 35,
                    BetonsterkteklasseEnum.C40_50 => 40,
                    BetonsterkteklasseEnum.C45_55 => 45,
                    BetonsterkteklasseEnum.C50_60 => 50,
                    BetonsterkteklasseEnum.C55_67 => 55,
                    BetonsterkteklasseEnum.C60_75 => 60,
                    BetonsterkteklasseEnum.C70_85 => 70,
                    BetonsterkteklasseEnum.C80_95 => 80,
                    BetonsterkteklasseEnum.C90_105 => 90,
                    //BetonsterkteklasseEnum.Eigen_Opgave => FckEigenOpgave,
                    _ => 40,
                };
            }
        }


        //(MPa)
        [TableColumn("kubusdruksterkte",
            Symbol = "<i>f</i><sub>ck,cube</sub>",
            Description = "is de karakteristieke kubusdruksterkte van beton na 28 dagen", Unit = "N/mm²",
            Article = "3.1.2 (3)"
            )]
        public double FckCube
        {
            get
            {
                return Betonsterkteklasse switch
                {
                    BetonsterkteklasseEnum.C12_15 => 15,
                    BetonsterkteklasseEnum.C16_20 => 20,
                    BetonsterkteklasseEnum.C20_25 => 25,
                    BetonsterkteklasseEnum.C25_30 => 30,
                    BetonsterkteklasseEnum.C30_37 => 37,
                    BetonsterkteklasseEnum.C35_45 => 45,
                    BetonsterkteklasseEnum.C40_50 => 50,
                    BetonsterkteklasseEnum.C45_55 => 55,
                    BetonsterkteklasseEnum.C50_60 => 60,
                    BetonsterkteklasseEnum.C55_67 => 67,
                    BetonsterkteklasseEnum.C60_75 => 75,
                    BetonsterkteklasseEnum.C70_85 => 85,
                    BetonsterkteklasseEnum.C80_95 => 95,
                    BetonsterkteklasseEnum.C90_105 => 105,
                    //BetonsterkteklasseEnum.Eigen_Opgave => FckCubeEigenOpgave,
                    _ => 50,
                };
            }
        }

        /// <summary>
        /// gemiddelde waarde van de cilinderdruksterkte van beton 
        /// </summary>
        [TableColumn("gem. cilinderdruksterkte", Symbol = "<i>f</i><sub>cm</sub>", Unit = "N/mm²",
            Description = "is de gemiddelde druksterkte op 28 dagen volgens tabel 3.1",
            Article = "3.1.2 (3)")]
        public double Fcm { get { return Fck + 8; } }
        public Formula FcmFormula => new("Tabel 3.1", @"f_{cm} = f_{ck} + 8", @$"f_{{cm}} = {Fck.ToEng()} + 8 = {Fcm.ToTeX()} ");


        /// <summary>
        /// gemiddelde waarde van de axiale treksterkte van beton 
        /// </summary>
        [TableColumn("gem. axiale treksterkte",
            Symbol = "<i>f</i><sub>ctm</sub>",
            Article = "3.1.2 (3)",
            Unit = "N/mm²",
            Description = "is de gemiddelde axiale treksterkte"
            )]

        public double Fctm
        {
            get
            {
                if (Fck <= 50.0)
                {
                    // Fctm tot C50/60
                    return 0.3 * Math.Pow(Fck, 2.0 / 3.0);
                }
                else
                {
                    // Fctm wanneer groter dan C50/60
                    return 2.12 * Math.Log(1.0 + (Fcm / 10.0));
                }
            }
        }

        public double SetFctmFl(double h)
        {
            var fctmfl = Math.Max((1.6 - (h / 1000.0)) * Fctm, Fctm);
            FctmFlFormula = new()
            {
                Name = "(3.23)",
                StaticValue = "f_{ctm,fl} = \\max \\left\\{ (1.6 - \\frac{h}{1000}) f_{ctm},\\; f_{ctm} \\right\\}",
                DynamicValue = $" = \\max \\left\\{{ (1.6 - \\frac{{{h:0}}}{{1000}} ) f_{{ctm}},\\; f_{{ctm}} \\right\\}}"
            };
            FctmFl = fctmfl;
            return fctmfl;
        }

        [TableColumn(Label = "gem. buigtrekstertke (zuivere buiging)", Symbol = "<i>f</i><sub>ctm,fl</sub>", Article = "3.1.8", Unit = "N/mm²")]
        public double FctmFl { get; private set; }
        public Formula FctmFlFormula { get; private set; } = new()
        {
            Name = "(3.23)",
            StaticValue = "f_{ctm,fl} = \\max \\left\\{ (1.6 - \\frac{h}{1000}) f_{ctm},\\; f_{ctm} \\right\\}",
        };

        public Formula FctmFormula
        {
            get
            {
                if (Fck <= 50)
                {
                    return new("Tabel 3.1", @"f_{ctm} = 0.3 \cdot f_{ck}^{2/3}", @$"= 0.3 \cdot {Fck.ToTeX()} ^{{2/3}} = {Fctm.ToTeX()} ");
                }
                else
                {
                    return new("Tabel 3.1", @"f_{ctm} = 2.12 \cdot \ln(1+(f_{cm}/10))", $@"= 2.12 \cdot \ln(1+({Fcm.ToTeX()}/10)) = {Fctm.ToTeX()}");
                }
            }
        }


        public int Tijdstip { get; set; } = 28;

        ///// <summary>
        ///// (3.4)
        ///// </summary>
        //public double FctmT
        //{
        //    get
        //    {
        //        return 
        //    }
        //}
        [TableColumn("treksterkte (5% fractiel)",
            Symbol = "<i>f<i><sub>ctk,0.05</sub>",
            Article = "3.1.2 (3)",
            Unit = "N/mm²")]
        public double FctkVijfProcent { get { return 0.7 * Fctm; } }    // 5% fractiel
        public Formula FctkVijfProcentFormula => new("Tabel 3.1",
            @"f_{ctk,0.05} = 0.7 \cdot f_{ctm}",
            @$"f_{{ctk,0.05}} = 0.7 \cdot {Fctm.ToEng()} = {FctkVijfProcent.ToEng()}");




        [TableColumn("treksterkte (95% fractiel)",
            Symbol = "<i>f<i><sub>ctk,0.95</sub>",
            Unit = "N/mm²",
            Article = "3.1.2 (3)")]
        public double FctkVijfEnNegentigProcent { get { return 1.3 * Fctm; } } // 95% fractiel
        public Formula FctkVijfEnNegentigProcentFormula => new("Tabel 3.1",
            @"f_{ctk,0.95} = 1.3 \cdot f_{ctm}",
            @$"f_{{ctk,0.95}} = 1.3 \cdot {Fctm.ToEng()} = {FctkVijfEnNegentigProcent.ToEng()}");


        public bool IsOntwerpSituatieBuitenGewoon = false;  // default Blijvend en tijdelijk conform art. 2.4.2.4 (1) Partiële factoren voor materialen 

        [JsonIgnore]
        [TableColumn("partiële factor beton",
            Symbol = "<i>ɣ</i><sub>C</sub>",
            Description = "is de partiële factor voor beton",
            Article = "2.4.2.4"
            )]
        public override double PartieleFactor { get => base.PartieleFactor; set => base.PartieleFactor = value; }



        [TableColumn("druksterkte", Symbol = "<i>f</i><sub>cd</sub>",
            Description = "is de rekenwaarde van de druksterkte",
            Article = "3.1.6 (1)P", Unit = "N/mm²")]
        public double Fcd { get { return AlphaCC * Fck / PartieleFactor; } }
        public Formula FcdFormula => new("(3.15)",
            @"f_{cd}=\alpha_{cc}f_{ck} / \gamma_{c}",
            @$"f_{{cd}}={AlphaCC} \cdot {Fck.ToEng()}/ {PartieleFactor} = {Fcd.ToEng()}");

        [TableColumn("treksterkte", Symbol = "<i>f</i><sub>ctd</sub>",
            Description = "is de rekenwaarde van de treksterkte",
            Article = "3.1.6 (2)P", Unit = "N/mm²")]
        public double Fctd { get { return AlphaCT * FctkVijfProcent / PartieleFactor; } }
        public Formula FctdFormula => new("(3.16)",
            @"f_{ctd}=\alpha_{ct}f_{ctk,0.05} / \gamma_{c}",
            @$"f_{{ctd}}={AlphaCT} \cdot {FctkVijfProcent.ToEng()}/ {PartieleFactor} = {Fctd.ToEng()}");


        public const double AlphaCT = 1; // 3.1.6 Dit is de coëfficiënt die rekening houdt met langeduureffecten op de treksterkte en met ongunstige effecten als gevolg van de manier waarop de belasting aangrijpt.
        public const double AlphaCC = 1; // 3.1.6 Dit is de coëfficiënt die rekening houdt met langeduureffecten op de druksterkte en met ongunstige effecten als gevolg van de manier waarop de belasting aangrijpt.

        /// <summary>
        /// De maxiale waarde van de druksterkte Cmax moet gelijk aan C90/105 zijn genomen.
        /// </summary>
        public const double FckMax = 90;
        /// <summary>
        /// De maxiale waarde van de druksterkte Cmax moet gelijk aan C90/105 zijn genomen.
        /// </summary>
        public const double FckCubeMax = 105;



        [JsonIgnore]
        public double Beta { get; set; }




        public double GetBeta()
        {
            SetAlphaBeta(this.EpsilonCu);
            return this.Beta;
        }



        public double GetAlpha()
        {
            SetAlphaBeta(this.EpsilonCu);
            return this.Alpha;

            //if (IsParabolischSpanningsRekDiagram)
            //    return this.GetFactorAlpha(EpsilonC2, EpsilonCu2);
            //else
            //    return this.GetFactorAlpha(EpsilonC3, EpsilonCu3);

        }

        public double GetEpsilonBreuk() { return this.GetEpsilonBetonBreuk(); }

        public double GetEpsilonStuik() { return this.GetEpsilonBetonStuik(); }

        public void SetBeta(double beta)
        {
            this.Beta = beta;
        }
        public void SetAlpha(double alpha)
        {
            this.Alpha = alpha;
        }

        public void SetAlphaBeta(double betonrek)
        {
            var (alpha, beta) = this.GetAlphaBeta(betonrek);
            this.Alpha = alpha;
            this.Beta = beta;
        }


        public override string ToString()
        {
            return this.BetonSterkteKlasseGebruiksvriendelijkeNaam;
        }



        protected override void Bereken()
        {
            // mogelijk berekeningen hier toevoegen
        }

        protected override bool Valideer()
        {
            // mogelijke validaties hier toevoegen
            return true;
        }

        //public override MarkupString ToHtml(bool isDraaiTabel = true)
        //{
        //    return this.ToHtmlTable(isDraaiTabel);
        //}

        [JsonIgnore]
        public double Alpha { get; set; }


        public double XudMax { get { return BetonContextExtensions.GetXudMax(Fcd, EpsilonCu3, BetonStaal.Fyd); } }

        public double Rho1Max { get { return this.GetRho1Max(); } }

        /// <summary>
        /// Let op! Ecm is in GPa 
        /// </summary>
        [TableColumn("secans-elasticiteitsmodulus",
            Symbol = "<i>E</i><sub>cm</sub>",
            Description = "is de secans-elasticiteitsmodulus van beton",
            Unit = "N/mm²",
            Article = "3.1.2 (3)")]
        public double Ecm { get { return this.GetEcm(); } }
        public Formula EcmFormula => new("Tabel 3.1",
            @"E_{cm}=22[f_{cm}/10]^{0.3} ×10^3",
            $@"E_{{cm}}=22[{Fcm.ToEng()}/10]^{{{0.3}}} ×10^3 = {Ecm.ToEng()}");



        [JsonIgnore]
        [TableColumn(label: "poissonverhouding",
            Article = "3.1.3 (4)",
            Symbol = "<i>ν</i>",
            Description = "De Poissonverhouding mag zijn gelijkgenomen aan 0,2 voor ongescheurd beton en aan 0 voor gescheurd beton")]
        public double PoissonFactor { get; private set; } = 0.2;


        public string Betonstuik
        {
            get { return EpsilonBetonStuik.GetFormattedStringPromille(); }
        }

        public string BetonstuikGrens
        {
            get { return EpsilonCu.GetFormattedStringPromille(); }
        }


        [TableColumn("betonstuik", Symbol = "<i>ε</i><sub>c,stuik</sub>",
            StringFormat = "{0:0.## ‰}",
            Article = "3.1.7",
            Description = "is de vervorming bij het bereiken van de maximale sterkte volgens tabel 3.1" +
            "<br />Deze waarde is afhankelijk van de gekozen spanning-rekrelatie:" +
            "<ul>" +
            "<li> <i>ε</i><sub>c2</sub> voor parabool-rechthoek</li>" +
            "<li> <i>ε</i><sub>c3</sub> voor bi-lineair </li>"

            )]
        public double EpsilonBetonStuik
        {
            get
            {
                return SpanningRekDiagram switch
                {
                    SpanningRekDiagramType.Parabolisch => EpsilonC2,
                    SpanningRekDiagramType.BiLineair => EpsilonC3,
                    _ => EpsilonC2
                };
            }
        }
        public Formula EpsilonBetonStuikFormula
        {
            get
            {
                if (SpanningRekDiagram == null) return new() { Name = "error", StaticValue = @"Onbekend spanning-rekdiagram" };
                if (SpanningRekDiagram == SpanningRekDiagramType.Parabolisch)
                {
                    if (Fck < 50)
                    {
                        return new()
                        {
                            Name = "Tabel 3.1",
                            StaticValue = @"\epsilon_{c2} = 2.0‰"

                        };
                    }
                    else
                    {
                        return new()
                        {
                            Name = "Tabel 3.1",
                            StaticValue = @"\epsilon_{c2} = 2.0+0.085(f_{ck-50})^{0.53}"
                        };
                    }

                }
                if (SpanningRekDiagram == SpanningRekDiagramType.BiLineair)
                {
                    if (Fck < 50)
                    {
                        return new()
                        {
                            Name = "Tabel 3.1",
                            StaticValue = @"\epsilon_{c3} = 1.75‰"

                        };
                    }
                    else
                    {
                        return new()
                        {
                            Name = "Tabel 3.1",
                            StaticValue = @"\epsilon_{c3} = 1.75+0.55[(f_{ck}-50)/40]"
                        };
                    }

                }

                // niets gevonden
                return new() { Name = "error", StaticValue = @"Onbekend spanning-rekdiagram" };


            }
        }



        [TableColumn("betonstuik grens", Symbol = "<i>ε</i><sub>cu</sub>",
            StringFormat = "{0:0.## ‰}",
            Article = "3.1.7",
            Description = "is de grenswaarde van de rek volgens tabel 3.1" +
            "<br />Deze waarde is afhankelijk van de gekozen spanning-rekrelatie:" +
            "<ul>" +
            "<li> <i>ε</i><sub>cu2</sub> voor parabool-rechthoek</li>" +
            "<li> <i>ε</i><sub>cu3</sub> voor bi-lineair </li>"
            )]

        public double EpsilonCu
        {
            get
            {
                return SpanningRekDiagram switch
                {
                    SpanningRekDiagramType.Parabolisch => EpsilonCu2,
                    SpanningRekDiagramType.BiLineair => EpsilonCu3,
                    _ => EpsilonCu2
                };
            }
        }
        public Formula EpsilonCuFormula
        {
            get
            {
                if (SpanningRekDiagram == null) return new() { Name = "error", StaticValue = @"Onbekend spanning-rekdiagram" };
                if (SpanningRekDiagram == SpanningRekDiagramType.Parabolisch)
                {
                    if (Fck < 50)
                    {
                        return new()
                        {
                            Name = "Tabel 3.1",
                            StaticValue = @"\epsilon_{cu2} = 3.5‰"

                        };
                    }
                    else
                    {
                        return new()
                        {
                            Name = "Tabel 3.1",
                            StaticValue = @"\epsilon_{cu2} = 2.6+35[(90-f_{ck}/100)]^4"
                        };
                    }
                }
                if (SpanningRekDiagram == SpanningRekDiagramType.BiLineair)
                {
                    if (Fck < 50)
                    {
                        return new()
                        {
                            Name = "Tabel 3.1",
                            StaticValue = @"\epsilon_{cu3} = 3.5‰"

                        };
                    }
                    else
                    {
                        return new()
                        {
                            Name = "Tabel 3.1",
                            StaticValue = @"\epsilon_{cu3} = 2.6+35[(90-f_{ck})/100]^4"
                        };
                    }
                }
                // niets gevonden

                return new() { Name = "error", StaticValue = @"Onbekend spanning-rekdiagram" };
            }
        }





        [Obsolete("Mag verwijderd worden?")]
        public double SigmaCd { get; set; }
        [Obsolete("Mag verwijderd worden?")]
        public double SigmaCk { get; set; }

        [Obsolete("Deze grafiek wordt niet gebruikt.")]
        public double EpsilonC1 { get { return this.GetEpsilonC1(); } }
        [Obsolete("Deze grafiek wordt niet gebruikt.")]
        public double EpsilonCu1 { get { return this.GetEpsilonCu1(); } }

        public double EpsilonC2 { get { return this.GetEpsilonC2(); } }
        public double EpsilonCu2 { get { return this.GetEpsilonCu2(); } }

        /// <summary>
        /// Alleen van toepassing bij parabool.
        /// </summary>
        /// 

        public double FactorN { get { return this.GetFactorN(); } }

        public double EpsilonC3 { get { return this.GetEpsilonC3(); } }
        public double EpsilonCu3 { get { return this.GetEpsilonCu3(); } }

        public double? EpsilonCuAangepast
        {
            get
            {
                if (BetonStaal.EpsilonS > BetonStaal.EpsilonUd)
                {
                    return this.GetBetonRekAangepast();
                }
                else return null;
            }
        }

        //public override string Eurocode => "EC2 - 3.1 Beton";

        public BetonContext(string sterkteklasse)
        {
            sterkteklasse = sterkteklasse.Replace("/", "_");
            _ = Enum.TryParse(sterkteklasse, out BetonsterkteklasseEnum sterkteklasseEnum);
            Betonsterkteklasse = sterkteklasseEnum;
        }


        public BetonContext(BetonsterkteklasseEnum klasse)
        {
            Betonsterkteklasse = klasse;
        }

        public BetonContext(BetonsterkteklasseEnum betonSterkteKlasse, BetonStaalKwaliteitEnum betonStaalKwaliteit)
        {
            Betonsterkteklasse = betonSterkteKlasse;
            BetonStaal = new BetonStaalContext(betonStaalKwaliteit);
        }

        public BetonContext(BetonsterkteklasseEnum betonSterkteKlasse, BetonStaalContext betonStaal)
        {
            Betonsterkteklasse = betonSterkteKlasse;
            BetonStaal = betonStaal;
        }
















    }

}
