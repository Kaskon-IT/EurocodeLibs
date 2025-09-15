using CommonLibrary;
using CommonLibrary.Extensions;
using ExportFactory.MigraDocContentModels;
using ExportFactory.Shared;

namespace Eurocode.BetonConstructies
{
    public partial class BetonContextKruipEnKrimpCalculator : BaseEurocodeContext
    {
        // 3.1.4 Kruip en krimp
        // (6) De totale krimpverkorting is samengesteld uit twee componenten:
        // de uitdrogingskrimpverkorting en de autogene krimpverkorting.

        /// <summary>
        /// Vergelijking (3.8)
        /// ε_cs
        /// </summary>
        /// 
        [TableColumn(Label = "totale krimpverkorting", Symbol = "<i>ε</i><sub>cs</sub>",
            Article = "3.1.4 (6)", Unit = "")]
        public double TotaleKrimpverkorting
        {
            get
            {
                return UitdrogingsKrimpverkorting + AutogeneKrimpverkorting;
            }
        }
        public Formula TotaleKrimpverkortingFormula => new("(3.8)",
            @"\epsilon_{cs} = \epsilon_{cd} + \epsilon_{ca}",
            $@"\epsilon_{{cs}} = {UitdrogingsKrimpverkorting.ToTeX()} + {AutogeneKrimpverkorting.ToTeX()} = {TotaleKrimpverkorting.ToTeX()}");





        /// <summary>
        /// Vergelijking (3.9)
        /// 
        /// </summary>
        [TableColumn(Label = "uitdrogingskrimpverkorting", Symbol = "<i>ε</i><sub>cd</sub>(t)",
            Article = "3.1.4 (6)", Description = "is de ontwikkeling van de uitdrogingskrimpverkorting in de tijd")]
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
        [TableColumn(Label = "factor uitdrogingskrimpverkorting", Symbol = $"β<sub>ds</sub>(t,t<sub>s</sub>)",
            Article = "3.1.4 (6)", Unit = "")]
        public double BetaDs
        {
            get
            {
                double deltaT = OuderdomBeton_t - OuderdomBetonBeginUitdrogingsKrimpOfZwelling_ts;
                double hulp = 0.04 * Math.Sqrt(Math.Pow(TheoretischeDikteBeton_h0, 3));
                return deltaT / (deltaT + hulp);
            }
        }
        public Formula BetaDsFormula => new("(3.10)",
            @"\beta_{ds}(t,t_s)=\frac{(t-t_s)}{(t-t_s) + 0.04 \sqrt{h_0^3} }",
            $@"\beta_{{ds}}(t,t_s)=\frac{{({OuderdomBeton_t.ToTeX()}-{OuderdomBetonBeginUitdrogingsKrimpOfZwelling_ts.ToTeX()})}}{{({OuderdomBeton_t.ToTeX()}-{OuderdomBetonBeginUitdrogingsKrimpOfZwelling_ts.ToTeX()}) + 0.04 \sqrt{{{TheoretischeDikteBeton_h0.ToTeX()}^3}} }} = {BetaDs.ToTeX()}");





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
                if (this.TheoretischeDikteBeton_h0 <= 100)
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
            Label = "autogene krimpverkorting",
            Symbol = "<i>ε</i><sub>ca</sub>(t)",
            Article = "3.1.4 (6)",
            Unit = "",
            Description = "is de autogene krimpverkorting"
            )]
        public double AutogeneKrimpverkorting
        {
            get
            {
                return BetaAS * EpsilonCaOneindig;
            }
        }
        public Formula AutogeneKrimpverkortingFormula => new("(3.11)",
            @"\epsilon_{ca} (t) = \beta_{as}(t) \cdot \epsilon_{ca}(\infty)",
            @$"\epsilon_{{ca}} ({OuderdomBeton_t:0.#}) = {BetaAS.ToTeX()} \cdot {EpsilonCaOneindig.ToTeX()} = {AutogeneKrimpverkorting.ToTeX()}");





        /// <summary>
        /// Art 3.1.4 (6)
        /// Vergelijking (3.12)
        /// </summary>
        [TableColumn(Label = "autogene krimpverkorting oneindig",
            Symbol = "<i>ε</i><sub>ca</sub>(∞)",
            Article = "3.1.4 (6)", Unit = "",
            Description = "is de autogene krimpverkorting bij oneindige tijd, wordt gebruikt in de formule voor autogene krimpverkorting")]
        public double EpsilonCaOneindig
        {
            get
            {
                return 2.5 * (_beton.Fck - 10) * 0.000001;
            }
        }
        public Formula EpsilonCaOneindigFormula => new("(3.12)",
            @"\epsilon_{ca}(\infty) = 2.5 (f_{ck}-10) 10^{-6})",
            $@"\epsilon_{{ca}}(\infty) = 2.5 ({_beton.Fck.ToTeX()}-10) 10^{{-6}}) = {EpsilonCaOneindig.ToTeX()}");


        /// <summary>
        /// Art 3.1.4 (6)
        /// Vergelijking (3.13)
        /// </summary>
        [TableColumn(Label = "factor", Symbol = "<i>β</i><sub>as</sub>(t)",
            Article = "3.1.4 (6)", Unit = "",
            Description = "wordt gebruikt in de formule voor autogene krimpverkorting")]
        public double BetaAS
        {
            get
            {
                return 1 - Math.Pow(Math.E, (-0.2 * Math.Pow(OuderdomBeton_t, 0.5)));
            }
        }
        public Formula BetaASFormula => new("(3.13)",
             $@"\beta_{{as}}(t)=1- exp (- 0.2 \cdot t^{{0.5}})",
            $@"\beta_{{as}}({OuderdomBeton_t:0.#})=1- exp (- 0.2 \cdot {{{OuderdomBeton_t:0.#}}}^{{0.5}}) = {BetaAS.ToTeX()}");



        /// <summary>
        /// 3.1.4 (6)
        /// OPMERKING De formule voor εcd,0 is gegeven in bijlage B.
        /// Basis verkorting uitdrogingskrimp
        /// Vergelijking (B.11)
        /// 
        /// </summary>
        [TableColumn(Label = "basis verkorting uitdrogingskrimp", Symbol = $"<i>ε</i><sub>cd,0</sub>",
            Article = "3.1.4 (6)",
            Unit = "",
            Description = "OPMERKING De formule voor <i>ε</i><sub>cd,0</sub> is gegeven in bijlage B.")]
        public double BasisVerkortingUitdrogingskrimp
        {
            get
            {
                double hulp1 = (220 + 110 * AlphaDs1);
                double macht = -AlphaDs2 * (_beton.Fcm / Fcmo);
                double hulp2 = Math.Pow(Math.E, macht);
                return 0.85 * (hulp1 * hulp2) * 0.000001 * BetaRH;
            }
        }
        public Formula BasisVerkortingUitdrogingskrimpFormula => new("(B.11)",
            @"\epsilon_{cd,0}=0.85 \left[ (220+110 \cdot \alpha_{ds1}) \cdot exp \left(-\alpha_{ds2} \cdot \frac{f_{cm}}{f_{cmo}} \right) \right] \cdot 10^{-6} \cdot \beta_{RH}",
            $@"\epsilon_{{cd,0}}=0.85 \left[ (220+110 \cdot {AlphaDs1.ToTeX()}) \cdot exp \left(-{AlphaDs2.ToTeX()} \cdot \frac{{{_beton.Fcm.ToTeX()}}}{{{Fcmo.ToTeX()}}}\right) \right] \cdot 10^{{-6}} \cdot {BetaRH.ToTeX()} = {BasisVerkortingUitdrogingskrimp.ToTeX()}");


        /// <summary>
        /// Vergelijking (B.12)
        /// Bijlage B
        /// </summary>
        /// 
        [TableColumn(Label = "factor luchtvochtigheid",
            Symbol = $"<i>β</i><sub>RH</sub>",
            Article = "Bijlage B",
            Description = "factor zie (B.12)")]
        public double BetaRH
        {
            get
            {

                double hulp = Math.Pow(RelatieveVochtigheid / RH0, 3);
                return 1.55 * (1 - hulp);
            }
        }
        public Formula BetaRHFormula => new("(B.12)",
            @"\beta_{RH} = 1.55 \left[ 1 - \left( \frac{RH}{RH_0} \right)^3 \right]",
            $@"\beta_{{RH}} = 1.55 \left[ 1 - \left( \frac{{{RelatieveVochtigheid.ToTeX()}}}{{{RH0.ToTeX()}}} \right)^3 \right] = {BetaRH.ToTeX()}");


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
        [TableColumn(Label = "factor cementsoort", Symbol = $"<i>α</i><sub>ds1</sub>", StringFormat = "{0:0}", Article = "Bijlage B")]
        public double AlphaDs1
        {
            get
            {
                switch (_beton.CementKlasse)
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
        [TableColumn(Label = "factor cementsoort", Symbol = $"<i>α</i><sub>ds2</sub>", StringFormat = "{0:0.00}", Article = "Bijlage B")]
        public double AlphaDs2
        {
            get
            {
                switch (_beton.CementKlasse)
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








        public override bool IsAkkoord()
        {
            return true;
        }

        protected override void Bereken()
        {
            //
        }

        protected override bool Valideer()
        {
            return IsAkkoord();
        }
    }
}
