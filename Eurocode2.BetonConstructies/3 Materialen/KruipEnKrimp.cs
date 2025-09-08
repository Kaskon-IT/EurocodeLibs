using CommonLibrary.Extensions;
using ExportFactory.Shared;
using System.Text.Json.Serialization;
using GREEK = ExportFactory.Services.GreekLetters;

namespace Eurocode.BetonConstructies
{
    public partial class BetonContext
    {
        // 3.1.4 Kruip en krimp

        // (1)P Kruip en krimp van beton hangen af van de vochtigheid van de omgeving, de afmetingen van het 
        // element en de samenstelling van het beton. Kruip wordt ook beïnvloed door de mate van verharding van
        // het beton op het moment dat de belasting voor het eerst is aangebracht en hangt af van de duur en de
        // grootte van de belasting.


        [TableColumn(HeaderText = "tangentmodulus", Symbol = "<i>E</i><sub>c</sub>", StringFormat = "{0:0.00 GPa}", Article = "3.1.4 (2)", Formula = @"E_{c} = 1.05 \cdot E_{cm}")]
        public double TangentModulus
        {
            get
            {
                return 1.05 * Ecm;
            }
        }


        //public string KruipEigenWaarde { get; set; } = "";

        public double? KruipEigenWaarde { get; set; } = null;



        /// <summary>
        /// (B.1)
        /// </summary>
        /// 
        [TableColumn(HeaderText = "kruipcoëfficiënt", Symbol = $"{GREEK.phi}<sub>(t,t<sub>0</sub>)</sub>", Article = "Bijlage B")]
        public double KruipCoefficient
        {
            get
            {
                if (KruipEigenWaarde == null)
                    return TheoretischeKruipCoefficient * BetaC;
                else
                    return KruipEigenWaarde.Value;
            }
        }
        public Formula KruipCoefficientFormula => new("(B.1)", @"\varphi(t,t_0) = \varphi_0 \cdot \beta_c(t,t_0)", $@"\varphi({OuderdomBeton_t},{OuderdomBetonOpMomentVanBelasten_t0.ToEng()}) = {TheoretischeKruipCoefficient.ToEng()} \cdot {BetaC.ToEng()} = {KruipCoefficient.ToEng(5)}");


        /// <summary>
        /// (B.2)
        /// </summary>
        [TableColumn(HeaderText = "theoretische kruipcoëfficiënt", Symbol = $"{GREEK.phi}<sub>0</sub>", Article = "Bijlage B")]
        public double TheoretischeKruipCoefficient
        {
            get
            {
                return FactorRelatieveVochtigheid * BetaFcm * BetaOuderdom;
            }
        }
        public Formula TheoretischeKruipCoefficientFormula => new("(B.2)", @"\varphi_0 = \varphi_{RH} \cdot \beta(f_{cm}) \cdot \beta(t_0)", @$"\varphi_0={FactorRelatieveVochtigheid.ToTeX()}\cdot{BetaFcm.ToTeX()}\cdot{BetaOuderdom.ToTeX()}={TheoretischeKruipCoefficient.ToTeX()}");


        /// <summary>
        /// (B.3a) en (B.3b)
        /// </summary>
        [TableColumn(HeaderText = "factor relatieve vochtigheid", Symbol = $"{GREEK.phi}<sub>RH</sub>",
                        Article = "Bijlage B")]

        public double FactorRelatieveVochtigheid
        {
            get
            {
                double hulp = (1 - (double)RelatieveVochtigheid / 100) / (0.1 * Math.Pow(TheoretischeDikteBeton_h0, 1.0 / 3.0));

                if (Fcm <= 35)
                {
                    // f_cm kleiner of gelijk aan 35 MPa (B.3a)
                    return 1 + hulp;
                }
                else
                {
                    // f_cm > 35 MPa (B.3b)
                    return (1 + hulp * CoefficentInvloedBetonsterkteAlpha1) * CoefficentInvloedBetonsterkteAlpha2;
                }
            }
        }

        public Formula FactorRelatieveVochtigheidFormula
        {
            get
            {
                string RH = $"{RelatieveVochtigheid.ToTeX()}";
                string h0 = $"{TheoretischeDikteBeton_h0.ToTeX()}";
                string waarde = $"{FactorRelatieveVochtigheid.ToTeX()}";

                if (Fcm <= 35)
                {
                    return new Formula("(B.3a)",
                        @"\varphi_{RH} = 1 + \frac{1 - RH / 100} {0.1 \cdot h_0^{1/3}}",

                        $@"\varphi_{{RH}} = 1 + \frac{{ 1 - {RH} / 100 }} {{ 0.1 \cdot {h0}^{{1/3}} }} = {waarde}");
                    //$@"\varphi_{RH} = 1 + \frac{1 - \frac{{{RH}}}{100}}{0.1 \cdot {h0}^{1/3}} = {FactorRelatieveVochtigheid.ToEng(5)}");
                }
                else
                {
                    return new Formula("(B.3b)",
                        @"\varphi_{RH} = \left[ 1 + \frac{1 - RH / 100} {0.1 \cdot h_0^{1/3}} \cdot \alpha_1 \right] \cdot \alpha_2",
                        $@"\varphi_{{RH}} = \left[ 1 + \frac{{ 1 - {RH} / 100 }} {{ 0.1 \cdot {h0}^{{1/3}} }} \cdot  {CoefficentInvloedBetonsterkteAlpha1.ToTeX()} \right] \cdot {CoefficentInvloedBetonsterkteAlpha2.ToTeX()}  = {waarde}");
                }
            }
        }





        public double CoefficentInvloedBetonsterkteAlpha1
        {
            get
            {
                return Math.Pow(35 / Fcm, 0.7);
            }
        }



        public double CoefficentInvloedBetonsterkteAlpha2
        {
            get
            {
                return Math.Pow(35 / Fcm, 0.2);
            }
        }

        public double CoefficentInvloedBetonsterkteAlpha3
        {
            get
            {
                return Math.Pow(35 / Fcm, 0.5);
            }
        }

        [TableColumn("macht cementsoort", Symbol = "α", Article = "Bijlage B")]
        public int MachtCementsoort
        {
            get
            {
                switch (CementKlasse)
                {
                    case CementklasseEnum.S: return -1;
                    default: return 0;
                    case CementklasseEnum.N: return 0;
                    case CementklasseEnum.R: return 1;
                }
            }
        }




        /// <summary>
        /// (B.4)
        /// een factor die rekening houdt met het effect van de betonsterkte op de theoretische kruipcoefficient.
        /// = 16,8 / Wortel(fcm)  
        /// </summary>
        /// 
        [TableColumn(HeaderText = "factor betonsterkte", Symbol = $"<i>{GREEK.beta}</i>(<i>f</i><sub>cm</sub>)", Article = "Bijlage B", Formula = @"\beta(f_{cm}) = \frac{16.8}{\sqrt{f_{cm}}}")]
        public double BetaFcm
        {
            get
            {
                return 16.8 / Math.Sqrt(Fcm);
            }
        }
        public Formula BetaFcmFormula => new("(B.4)", @"\beta(f_{cm}) = \frac{16.8}{\sqrt{f_{cm}}}", $@"\beta(f_{{cm}}) = \frac{{16.8}}{{\sqrt{{{Fcm.ToTeX()}}}}} = {BetaFcm.ToTeX()}");





        /// <summary>
        /// (B.5)
        /// een factor die rekening houdt met het effect van de ouderdom van het beton op het tijdstip van belasten op de theoretische kruipcoefficient.
        /// </summary>
        [TableColumn(HeaderText = "factor ouderdom", Symbol = $"<i>{GREEK.beta}</i>(t<sub>0</sub>)", StringFormat = "{0:0.0000}", Article = "Bijlage B", Formula = @"\beta(t_{0}) = \frac{1}{0,1 + t_{0}^{0,20}}")]
        public double BetaOuderdom
        {
            get
            {
                return 1 / (0.1 + Math.Pow(OuderdomInclusiefCement, 0.20));

            }
        }
        public Formula BetaOuderdomFormula => new("(B.5)", @"\beta(t_{0}) = \frac{1}{0,1 + t_{0}^{0,20}}", $@"\beta(t_{{0}}) = \frac{{1}}{{0,1 + {OuderdomInclusiefCement.ToTeX()}^{{0,20}}}} = {BetaOuderdom.ToTeX()}");



        [TableColumn(
            HeaderText = "relatieve vochtigheid", Symbol = $"RH", StringFormat = "{0:0 }%")]
        public int RelatieveVochtigheid { get; set; } = 50;






        [JsonIgnore]
        public int OuderdomBeton_t { get; set; } = 18250;

        [TableColumn(HeaderText = "ouderdom beton bij belasten", Symbol = $"<i>t</i><sub>0</sub>", StringFormat = "{0:0}", Article = "")]
        public int OuderdomBetonOpMomentVanBelasten_t0 { get; set; } = 30;




        public double OmtrekElementInAanrakingMetBuitenlucht_u
        {
            get
            {
                return 2 * (this.Profiel.Breedte + this.Profiel.Hoogte); // let op! fixed rechthoek
            }
        }

        /// <summary>
        /// (B.6)
        /// </summary>
        /// 
        [TableColumn(HeaderText = "theoretische dikte", Symbol = "<i>h</i><sub>0</sub>", Unit = "mm", Article = "Bijlage B")]
        public double TheoretischeDikteBeton_h0
        {
            get
            {
                double u = OmtrekElementInAanrakingMetBuitenlucht_u;
                return 2 * this.Profiel.Area / u;
                //return 2 * OppervlakteDwarsdoorsnedeBeton_Ac / OmtrekDeelDwarsdoorsnedeBlootgesteldAanUitdroging_u;
            }
        }
        public Formula TheoretischeDikteBeton_h0Formula => new("(B.6)", @"h_0=\frac{2\cdot A_c}{u}", @$"h_0=\frac{{ {2} \cdot {Profiel.Area.ToTeX()} }}  {{ {OmtrekElementInAanrakingMetBuitenlucht_u.ToTeX()} }} ");


        // \beta_c(t, t_{0}) = \left[ \frac{t - t_{0}}{\beta_H + t - t_{0}} \right]^{0,3}

        [TableColumn(HeaderText = "coefficient", Symbol = "<i>β</i><sub>c</sub>(t,t<sub>0</sub>)")]
        public double BetaC
        {
            get
            {
                return Math.Pow((double)(OuderdomBeton_t - OuderdomBetonOpMomentVanBelasten_t0) / (double)(BetaH + OuderdomBeton_t - OuderdomBetonOpMomentVanBelasten_t0), 0.3);
            }
        }


        public Formula BetaCFormula
        {
            get
            {
                string t = $"{OuderdomBeton_t.ToTeX()}";
                string t0 = $"{OuderdomBetonOpMomentVanBelasten_t0.ToTeX()}";


                return new()
                {
                    StaticValue = @$"\beta_c(t,t_0) = \left[ \frac{{(t-t_0)}} {{(\beta_H+t-t_0)}} \right]^{{0.3}}",
                    DynamicValue = @$"\beta_c({t},{t0}) = \left[ \frac{{({t}-{t0})}} {{({BetaH:0.###}+{t}-{t0})}} \right]^{{0.3}} = {BetaC.ToTeX()}",
                    Name = "(B.7)"
                };
            }
        }



        [TableColumn(HeaderText = "coefficient", Symbol = "<i>β</i><sub>H</sub>", Article = "Bijlage B")]
        public double BetaH
        {
            get
            {
                if (Fcm <= 35)
                {
                    var value1 = 1.5 * (1 + Math.Pow(0.012 * RelatieveVochtigheid, 18)) * TheoretischeDikteBeton_h0 + 250;
                    var value2 = 1500;
                    return Math.Min(value1, value2);
                }
                else
                {
                    var value1 = 1.5 * (1 + Math.Pow(0.012 * RelatieveVochtigheid, 18)) * TheoretischeDikteBeton_h0 + 250 * CoefficentInvloedBetonsterkteAlpha3;
                    var value2 = 1500 * CoefficentInvloedBetonsterkteAlpha3;
                    return Math.Min(value1, value2);
                }
            }
        }


        public Formula BetaHFormula
        {
            get
            {
                string RH = $"{RelatieveVochtigheid.ToTeX()}";
                string h0 = $"{TheoretischeDikteBeton_h0.ToTeX()}";




                if (Fcm <= 35)
                {

                    return new()
                    {
                        Name = "(B.8a)",
                        StaticValue = @$"\beta_H = 1.5 \left[ 1 + (0.012 \cdot RH)^{{18}} \right] h_0 + 250 \leq 1500",
                        DynamicValue = @$"\beta_H = 1.5 \left[ 1 + (0.012 \cdot {RH})^{{18}} \right] {h0} + 250 \leq 1500 = {BetaH.ToTeX()}",
                    };
                }
                else
                {
                    string alpha3 = $"{CoefficentInvloedBetonsterkteAlpha3.ToTeX()}";

                    return new()
                    {
                        Name = "(B.8b)",
                        StaticValue = @$"\beta_H = 1.5 \left[ 1 + (0.012 \cdot RH)^{{18}} \right] h_0 + 250 \cdot \alpha_3 \leq 1500 \cdot \alpha_3",
                        DynamicValue = @$"\beta_H = 1.5 \left[ 1 + (0.012 \cdot {RH})^{{18}} \right] {h0} + 250 \cdot {alpha3} \leq 1500 \cdot {alpha3} = {BetaH.ToTeX()}",
                    };
                }

            }
        }








        [TableColumn(HeaderText = "periodes", Symbol = "<i>Δ</i><sub>ti</sub>,<i>T</i>")]
        public string PeriodesSamenvatting => TemperatuurPeriodes == null || TemperatuurPeriodes.Count == 0
            ? "n.v.t."
            : string.Join(" + ",
            TemperatuurPeriodes.Select(p =>
                $"{p.AantalDagen} dagen ({p.TemperatuurGedurendeDezeDagen}°C)"));










        public List<TemperatuurPeriode> TemperatuurPeriodes { get; set; } =
            [
                new(){AantalDagen = 10, TemperatuurGedurendeDezeDagen = 8},
                new(){AantalDagen = 8, TemperatuurGedurendeDezeDagen = 14},
                new(){AantalDagen = 10, TemperatuurGedurendeDezeDagen = 17}
            ];





        /// <summary>
        /// (B.9)
        /// </summary>
        /// 
        [TableColumn(HeaderText = "belastingduur (effect cementsoort)", Symbol = $"<i>t</i><sub>0</sub>", Article = "Bijlage B", Unit = "dagen")]
        public double OuderdomInclusiefCement
        {
            get
            {
                // NB. (B.10) is WEL voorzien. Indien niet gewenst geen temperatuur-periodes opgeven.
                return this.GetOuderdomVergelijkingB9(OuderdomBeton_t0T);
            }
        }

        public Formula OuderdomInclusiefCementFormula => new("(B.9)", @"t_0 = t_{0,T} \cdot \left( \frac{9}{2 + t_{0,T}^{1.2}} + 1 \right)^{\alpha} \geq 0.5",
            $@"t_0 = {OuderdomBeton_t0T.ToTeX()} \cdot \left( \frac{{9}}{{2 + {OuderdomBeton_t0T.ToTeX()}^{{1.2}}}} + 1 \right)^{{{MachtCementsoort}}} = {OuderdomInclusiefCement.ToTeX()}");



        /// <summary>
        /// (B.10)
        /// </summary>
        [TableColumn(HeaderText = "ouderdom beton", Symbol = $"<i>t</i><sub>0T</sub>", Article = "Bijlage B", Unit = "dagen",
            Formula = @"t_{T}=\sum\limits_{i=1}^n e^{-(4000/[273 + T(\Delta t_i )] - 13,65)} \cdot \Delta t_i")]
        public double OuderdomBeton_t0T
        {
            get
            {
                if (TemperatuurPeriodes == null || TemperatuurPeriodes.Count == 0)
                    return OuderdomBetonOpMomentVanBelasten_t0;
                else
                {
                    double totaal = 0;
                    foreach (var item in TemperatuurPeriodes)
                    {
                        totaal += item.GecorigeerdeOuderdom_tT;
                    }
                    return totaal;
                }
            }
        }


        public Formula OuderdomBeton_t0TFormula
        {
            get
            {
                string dynVal = $"n.v.t";
                List<string> gecorigeerdeDagen = [];
                foreach (var p in TemperatuurPeriodes)
                {
                    gecorigeerdeDagen.Add(p.GecorigeerdeOuderdom_tT.ToString("0.00"));

                }

                if (gecorigeerdeDagen.Count > 0)
                {
                    dynVal = string.Join(" + ", gecorigeerdeDagen);
                }


                return new()
                {
                    Name = "(B.10)",
                    StaticValue = @"t_{T}=\sum\limits_{i=1}^n e^{-(4000/[273 + T(\Delta t_i )] - 13,65)} \cdot \Delta t_i",
                    // geen dynamische waarde is een som kan eventueel weergeven met itteratie door temperaturen
                    DynamicValue = @"t_{T}=\sum\limits_{i=1}^n =" + dynVal + "=" + OuderdomBeton_t0T.ToTeX(unit: "dagen")
                };
            }

        }

        public int OuderdomBetonBeginUitdrogingsKrimpOfZwelling_ts { get; set; } = 1;

        [JsonIgnore] // wordt ingelezen // todo controleer of goed gaat
        public ParametrischeProfielen.ParametrischProfielContext Profiel { get; set; } = new(500, 700);


        /// <summary>
        /// 3.1.8 Buigtreksterkte
        /// (1) De gemiddelde buigtreksterkte van gewapend-betonelementen hangt af van de gemiddelde axiale treksterkte en de hoogte van de dwarsdoorsnede. De volgende relatie mag zijn gebruikt:
        /// Formule (3.23)
        /// </summary>
        public double FctmFl
        {
            get
            {
                double val1 = (1.6 - Profiel.Hoogte / 1000) * this.Fctm;
                double val2 = this.Fctm;
                return Math.Max(val1, val2);
            }
        }


        /// <summary>
        /// de oppervalkte van de dwarsdoorsnede.
        /// VERVALLEN --> ProfielContext Profiel
        /// </summary>
        //[Obsolete("Vervallen, gebruik Profiel.Area")]
        //public double OppervlakteDwarsdoorsnedeBeton_Ac { get; set; } = 500 * 700;

        /// <summary>
        /// de omtrek van het element dat in aanraking komt met de buitenlucht.
        /// VERVALLEN --> ProfielContext Profiel
        /// </summary>
        //[Obsolete("Vervallen, gebruik Profiel.Perimeter")]
        //public double OmtrekDeelDwarsdoorsnedeBlootgesteldAanUitdroging_u { get; set; } = 2 * (500 + 700);








        // 3.1.4 Kruip en krimp
        // (6) De totale krimpverkorting is samengesteld uit twee componenten:
        // de uitdrogingskrimpverkorting en de autogene krimpverkorting.

        /// <summary>
        /// Vergelijking (3.8)
        /// </summary>
        /// 
        [TableColumn(HeaderText = "totale krimpverkorting",
            Symbol = "<i>ε</i><sub>cs</sub>",
            Article = "3.1.4",
            Formula = @"\epsilon_{cs} = \epsilon_{cd} + \epsilon_{ca}")]
        public double TotaleKrimpverkorting
        {
            get
            {
                return UitdrogingsKrimpverkorting + AutogeneKrimpverkorting;
            }
        }
        public Formula TotaleKrimpverkortingFormula => new("(3.8)", @"\epsilon_{cs} = \epsilon_{cd} + \epsilon_{ca}", $@"\epsilon_{{cs}} = {UitdrogingsKrimpverkorting.ToTeX()} + {AutogeneKrimpverkorting.ToTeX()} = {TotaleKrimpverkorting.ToTeX()}");





        /// <summary>
        /// Vergelijking (3.9)
        /// 
        /// </summary>
        [TableColumn(HeaderText = "uitdrogingskrimpverkorting", Symbol = "<i>ε</i><sub>cd</sub>(t)", Article = "3.1.4")]
        public double UitdrogingsKrimpverkorting
        {
            get
            {
                return BetaDs * CoefficientFictieveHoogte * BasisVerkortingUitdrogingskrimp;
            }
        }
        public Formula UitdrogingsKrimpverkortingFormula => new("(3.9)", @"\epsilon_{cd}(t)= \beta_{ds}(t,t_s) \cdot k_h \cdot \epsilon_{cd,0}", $@"\epsilon_{{cd}}(t)= {BetaDs.ToTeX()} \cdot {CoefficientFictieveHoogte.ToTeX()} \cdot {BasisVerkortingUitdrogingskrimp.ToTeX()} = {UitdrogingsKrimpverkorting.ToTeX()}");




        /// <summary>
        /// Vergelijking (3.10)
        /// Wordt gebruikt in vergelijking (3.9)
        /// Factor gebruikt bij bepalen van de uitdrogingskrimpverkorting
        /// </summary>
        [TableColumn(HeaderText = "factor uitdrogingskrimpverkorting", Symbol = $"{GREEK.beta}<sub>ds</sub>(t,t<sub>s</sub>)", Article = "3.1.4")]
        public double BetaDs
        {
            get
            {
                double deltaT = OuderdomBeton_t - OuderdomBetonBeginUitdrogingsKrimpOfZwelling_ts;
                double hulp = 0.04 * Math.Sqrt(Math.Pow(TheoretischeDikteBeton_h0, 3));
                return deltaT / (deltaT + hulp);
            }
        }
        public Formula BetaDsFormula => new("(3.10)", @"\beta_{ds}(t,t_s)=\frac{(t-t_s)}{(t-t_s) + 0.04 \sqrt{h_0^3} }", $@"\beta_{{ds}}(t,t_s)=\frac{{({OuderdomBeton_t:0.#}-{OuderdomBetonBeginUitdrogingsKrimpOfZwelling_ts:0.#})}}{{({OuderdomBeton_t.ToTeX()}-{OuderdomBetonBeginUitdrogingsKrimpOfZwelling_ts.ToTeX()}) + 0.04 \sqrt{{{TheoretischeDikteBeton_h0:0.###}^3}} }} = {BetaDs.ToTeX()}");





        /// <summary>
        /// Tabel 3.3
        /// </summary>
        public double CoefficientFictieveHoogte
        {
            get
            {
                // 100 - 1.00
                // 200 - 0.85
                // 300 - 0.75
                // >500 - 0.70
                if (TheoretischeDikteBeton_h0 <= 100)
                    return 1.00;
                else if (TheoretischeDikteBeton_h0 <= 200)
                {
                    return 1.00 - (TheoretischeDikteBeton_h0 - 100.0) / 100.0 * 0.15;
                }
                else if (TheoretischeDikteBeton_h0 <= 300)
                {
                    return 0.85 - (TheoretischeDikteBeton_h0 - 200.0) / 100.0 * 0.10;
                }
                else if (TheoretischeDikteBeton_h0 <= 500)
                {
                    return 0.75 - (TheoretischeDikteBeton_h0 - 300.0) / 200.0 * 0.05;
                }
                else return 0.70;

            }
        }


        /// <summary>
        /// Vergelijking (3.11)
        /// De autogene krimpverkorting
        /// Is BetaAS * EpsilonCaOneindig
        /// </summary>
        [TableColumn(
            HeaderText = "autogene krimpverkorting",
            Symbol = "<i>ε</i><sub>ca</sub>(t)",
            StringFormat = "{0:0.000E+00}",
            Article = "3.1.4",
            Formula = @"\epsilon_{ca} (t) = \beta_{as}(t) \cdot \epsilon_{ca}(\infty)",
            DynamicFormulaProperty = nameof(AutogeneKrimpverkortingDynamicFormula)
            )]
        public double AutogeneKrimpverkorting
        {
            get
            {
                return BetaAS * EpsilonCaOneindig;
            }
        }
        public Formula AutogeneKrimpverkortingFormula => new("(3.11)", @"\epsilon_{ca} (t) = \beta_{as}(t) \cdot \epsilon_{ca}(\infty)", @$"\epsilon_{{ca}} (t) = {BetaAS:E2} \cdot {EpsilonCaOneindig.ToTeX()} = {AutogeneKrimpverkorting:E2}");


        public string AutogeneKrimpverkortingDynamicFormula => @$"\epsilon_{{ca}} (t) = {BetaAS:E2} \cdot {EpsilonCaOneindig.ToTeX()} = {AutogeneKrimpverkorting:E2}";



        /// <summary>
        /// Vergelijking (3.12)
        /// Deze wordt gebruikt in vergelijking (3.11)
        /// = 2,5 (fck – 10) ×10-6
        /// </summary>
        [TableColumn(HeaderText = "", Symbol = "<i>ε</i><sub>ca</sub>(∞)",
            StringFormat = "{0:0.000E0}",
            Article = "(3.12)",
            Formula = @"\epsilon_{ca}(\infty) = 2.5 (f_{ck}-10) 10^{-6})"
            )]
        public double EpsilonCaOneindig
        {
            get
            {
                return 2.5 * (Fck - 10) * 0.000001;
            }
        }
        public Formula EpsilonCaOneindigFormula => new("3.12", @"\epsilon_{ca}(\infty) = 2.5 (f_{ck}-10) 10^{-6})", @"\epsilon_{ca}(\infty) = 2.5 (f_{ck}-10) 10^{-6})");


        /// <summary>
        /// Vergelijking (3.13)
        /// = 1 – exp (– 0,2 t ^0,5)
        /// </summary>
        [TableColumn(HeaderText = "", Symbol = "<i>β</i><sub>as</sub>(t)",
            StringFormat = "{0:0.000E0}",
            Article = "(3.13)",
            Formula = @"\beta_{as}(t)=1- exp (- 0.2 t^{0.5})"
            )]
        public double BetaAS
        {
            get
            {
                return 1 - Math.Pow(Math.E, (-0.2 * Math.Pow(OuderdomBeton_t, 0.5)));
            }
        }


        /// <summary>
        /// Basis verkorting uitdrogingskrimp
        /// Vergelijking (B.11)
        /// 
        /// </summary>
        [TableColumn(HeaderText = "basis verkorting uitdrogingskrimp", Symbol = $"<i>{GREEK.Epsilon}</i><sub>cd,0</sub>",
            StringFormat = "{0:0.000E0}",
            Article = "(B.11)",
            Formula = @"\epsilon_{cd,0}=0.85 \left[ (220+110 \cdot \alpha_{ds1}) \cdot exp(-\alpha_{ds2} \cdot \frac{f_{cm}}{f_{cmo}}) \right] \cdot 10^{-6} \cdot \beta_{RH}")]
        public double BasisVerkortingUitdrogingskrimp
        {
            get
            {
                double hulp1 = (220 + 110 * AlphaDs1);
                double macht = -AlphaDs2 * (Fcm / Fcmo);
                double hulp2 = Math.Pow(Math.E, macht);
                return 0.85 * (hulp1 * hulp2) * 0.000001 * BetaRH;
            }
        }

        /// <summary>
        /// Vergelijking (B.12)
        /// Wordt gebruikt in vergelijking (B.11)
        /// Bijlage B.2
        /// </summary>
        /// 
        [TableColumn(HeaderText = "?", Symbol = $"{GREEK.beta}<sub>RH</sub>", StringFormat = "{0:0.0000}", Article = "(B.12)")]
        public double BetaRH
        {
            get
            {

                double hulp = Math.Pow(RelatieveVochtigheid / RH0, 3);
                return 1.55 * (1 - hulp);
            }
        }

        [TableColumn(HeaderText = "", Symbol = $"<i>RH</i><sub>0</sub>", StringFormat = "{0:0}%", Article = "")]
        public double RH0
        {
            get
            {
                return 100;
            }
        }

        /// <summary>
        /// Gebruikt in vergelijking (B.11)
        /// Bijlage B.2 Basisvergelijkingen voor het bepalen van de verkorting ten gevolge van uitdrogingskrimp
        /// </summary>
        [TableColumn(HeaderText = "", Symbol = $"<i>f</i><sub>cmo</sub>", StringFormat = "{0:0}", Article = "")]
        public double Fcmo
        {
            get
            {
                return 10;
            }
        }

        /// <summary>
        /// is een coëfficiënt die afhangt van de cementsoort
        /// Gebruikt in vergelijking (B.11) 
        /// Bijlage B.2 Basisvergelijkingen voor het bepalen van de verkorting ten gevolge van uitdrogingskrimp
        /// </summary>
        [TableColumn(HeaderText = "", Symbol = $"<i>α</i><sub>ds1</sub>", StringFormat = "{0:0}", Article = "")]
        public double AlphaDs1
        {
            get
            {
                switch (CementKlasse)
                {
                    case CementklasseEnum.S:
                        return 3;
                    default:
                    case CementklasseEnum.N:
                        return 4;
                    case CementklasseEnum.R:
                        return 6;
                }
            }
        }


        /// <summary>
        /// is een coëfficiënt die afhangt van de cementsoort
        /// Gebruikt in vergelijking (B.11)
        /// Bijlage B.2 Basisvergelijkingen voor het bepalen van de verkorting ten gevolge van uitdrogingskrimp
        /// </summary>
        [TableColumn(HeaderText = "", Symbol = $"<i>α</i><sub>ds2</sub>", StringFormat = "{0:0.00}", Article = "")]
        public double AlphaDs2
        {
            get
            {
                switch (CementKlasse)
                {
                    case CementklasseEnum.S:
                        return 0.13;
                    default:
                    case CementklasseEnum.N:
                        return 0.12;
                    case CementklasseEnum.R:
                        return 0.11;
                }
            }
        }

        /// <summary>
        /// De effectieve elasticiteitsmodulus van beton. (inclusief kruip)
        /// Ec,eff = Ecm / (1 + KruipCoefficient)
        /// </summary>
        [TableColumn(HeaderText = "effectieve elasticiteitsmodulus", Symbol = $"<i>E</i><sub>c,eff</sub>", StringFormat = "{0:0.00}", Article = "?")]
        public double EcEff
        {
            get
            {
                return Ecm / (1 + KruipCoefficient);
            }
        }

        [TableColumn(HeaderText = "verhouding staal/beton", Symbol = $"<i>α</i><sub>e</sub>", StringFormat = "{0:0.0000}", Article = "?")]
        public double Alphae
        {
            get
            {
                return BetonStaalContext.Es / (EcEff * 1000);
            }
        }




    }


    public class TemperatuurPeriode
    {
        public int AantalDagen { get; set; } = 1;
        public double TemperatuurGedurendeDezeDagen { get; set; } = 20;

        // =EXP(-(4000/(273+ Temperatuur ) - 13,65 )) * Dagen
        public double GecorigeerdeOuderdom_tT => Math.Exp(-(4000 / (273 + TemperatuurGedurendeDezeDagen) - 13.65)) * AantalDagen;

    }


    public static class KruipEnKrimpExtensions
    {
        public static double GetOuderdomVergelijkingB9(this BetonContext context, double t0t)
        {
            //(2) Het effect van de cementsoort(zie 3.1.2(6)) op de kruipcoëfficiënt van het beton mag in rekening zijn
            //gebracht door het aanpassen van de duur van de belasting t0 in vergelijking(B.5) volgens de volgende
            //vergelijking: 

            double minValue = 0.5;
            var helper = 2 + Math.Pow(t0t, 1.2);
            double value = t0t * Math.Pow(9 / helper + 1, context.MachtCementsoort);

            return Math.Max(minValue, value);

        }





    }

    public class FormulaDelete
    {

    }

}
