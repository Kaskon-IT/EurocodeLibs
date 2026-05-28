using CommonLibrary;
using Eurocode.Belastingen;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Eurocode.BetonConstructies
{
    /// <summary>
    /// Berekening van de M-N interactie voor een rechthoekige doorsnede met enkelvoudige wapening.
    /// Genereert een interactiediagram [NEd, MRd] voor verschillende normaalkrachtwaarden.
    /// </summary>
    public class BuigingMetNormaalkracht : BaseEurocodeContext
    {
        public BuigingMetNormaalkracht()
        {
        }

        public override string Heading { get; set; } = "Buiging met normaalkracht";

        public override void Init()
        {
            base.Init();
            // Initialiseer interactiepunten
            InteractiePunten = [];
        }

        // ================================================================
        // Input Contexten
        // ================================================================

        public BetonContext Beton { get; set; } = new(BetonsterkteklasseEnum.C30_37);

        // ================================================================
        // Input Properties - Geometrie
        // ================================================================

        private double _breedte = 500;
        private double _hoogte = 1000;
        private double _as  = 5000; // mm² trekwapening (onderkant)
        private double _as2 = 5000; // mm² drukwapening (bovenkant)
        private double _d2 = 50;   // afstand hart wapening tot rand (symmetrisch)

        [TableColumn("Breedte doorsnede", Symbol = "<i>b</i>", Unit = "mm")]
        public double Breedte
        {
            get => _breedte;
            set
            {
                if (SetAndRecalculate(ref _breedte, value))
                {
                    OnPropertyChanged(nameof(NuttigeHoogte));
                }
            }
        }

        [TableColumn("Hoogte doorsnede", Symbol = "<i>h</i>", Unit = "mm")]
        public double Hoogte
        {
            get => _hoogte;
            set
            {
                if (SetAndRecalculate(ref _hoogte, value))
                {
                    OnPropertyChanged(nameof(NuttigeHoogte));
                }
            }
        }

        [TableColumn("Wapeningsoppervlak trekzijde", Symbol = "<i>A</i><sub>s</sub>", Unit = "mm²")]
        public double As
        {
            get => _as;
            set => SetAndRecalculate(ref _as, value);
        }

        [TableColumn("Wapeningsoppervlak drukzijde", Symbol = "<i>A</i><sub>s2</sub>", Unit = "mm²")]
        public double As2
        {
            get => _as2;
            set => SetAndRecalculate(ref _as2, value);
        }

        [TableColumn("Afstand hart wapening tot rand", Symbol = "<i>d</i><sub>2</sub>", Unit = "mm")]
        public double D2
        {
            get => _d2;
            set
            {
                if (SetAndRecalculate(ref _d2, value))
                {
                    OnPropertyChanged(nameof(NuttigeHoogte));
                }
            }
        }

        // ================================================================
        // Input Properties - Belastingen
        // ================================================================

        private double _nEd = 0;   // Normaalkracht (positief = druk)
        private double _mEd = 100; // Moment

        [TableColumn("Rekenwaarde normaalkracht", Symbol = "<i>N</i><sub>Ed</sub>", Unit = "kN")]
        public double NEd
        {
            get => _nEd;
            set => SetAndRecalculate(ref _nEd, value);
        }

        [TableColumn("Rekenwaarde moment", Symbol = "<i>M</i><sub>Ed</sub>", Unit = "kNm")]
        public double MEd
        {
            get => _mEd;
            set => SetAndRecalculate(ref _mEd, value);
        }

        // ================================================================
        // Output Properties
        // ================================================================

        [TableColumn(Label = "Nuttige hoogte", Symbol = "<i>d</i>", Unit = "mm")]
        public double NuttigeHoogte => Hoogte - D2;

        /// <summary>
        /// Lijst van (N, M) punten die de interactiecurve vormen.
        /// N in kN (positief = druk), M in kNm.
        /// </summary>
        public List<(double N, double M)> InteractiePunten { get; private set; } = [];

        /// <summary>
        /// Unity Check: verhouding tussen belasting en weerstand.
        /// UC = √((NEd/NRd)² + (MEd/MRd)²) voor lineaire benadering.
        /// Waarde &lt; 1.0 = OK.
        /// </summary>
        [TableColumn(Label = "Unity Check", Symbol = "<i>UC</i>", Unit = "-")]
        public double UnityCheck { get; private set; }

        /// <summary>
        /// Momentweerstand bij opgegeven normaalkracht NEd.
        /// </summary>
        [TableColumn(Label = "Momentweerstand", Symbol = "<i>M</i><sub>Rd</sub>", Unit = "kNm")]
        public double MRd { get; private set; }

        /// <summary>
        /// Normaalkrachtweerstand bij opgegeven moment MEd (niet altijd relevant).
        /// </summary>
        [TableColumn(Label = "Normaalkrachtweerstand", Symbol = "<i>N</i><sub>Rd</sub>", Unit = "kN")]
        public double NRd { get; private set; }

        // ================================================================
        // Berekening
        // ================================================================

        protected override void Bereken()
        {
            // Bereken interactiediagram
            BerekenInteractieDiagram();

            // Bereken weerstand voor opgegeven NEd
            MRd = BerekenMomentWeerstand(NEd);

            // Unity check: MEd / MRd
            UnityCheck = MRd > 0 ? MEd / MRd : double.MaxValue;
        }

        protected override bool Valideer()
        {
            // Validatie: Unity Check moet <= 1.0 zijn
            bool isValid = UnityCheck <= 1.0;

            if (!isValid)
            {
                AddMeldingWaarschuwing($"Unity Check ({UnityCheck:F2}) overschrijdt 1.0 - doorsnede voldoet niet!");
            }

            return isValid;
        }

        // ================================================================
        // Private Helper Methods
        // ================================================================

        /// <summary>
        /// Berekent de M-N interactiecurve conform EC2 fig. 4.1.
        /// Structuur:
        ///   Geval 1:  εcu3 boven, trekwapening juist vloeiend (εs = εyd) → balansdiepte x1
        ///   Geval 2:  εcu3 boven, neutrale as op onderkant doorsnede (x = h)
        ///   Geval 3:  volledig gedrukt, rekverdeling roteert om pivot yp
        ///   Geval 4:  zuivere druk, εc3 uniform
        /// Per regime worden tussenliggende punten toegevoegd.
        /// </summary>
        private void BerekenInteractieDiagram()
        {
            InteractiePunten.Clear();

            double fcd  = Beton.Fcd;
            double fyd  = Beton.BetonStaal.Fyd;
            double Es   = Beton.BetonStaal.ElasticiteitsModulus;
            double ecu3 = Beton.EpsilonCu3;
            double ec3  = Beton.EpsilonC3;

            double b   = Breedte;
            double h   = Hoogte;
            double d   = NuttigeHoogte;   // trekwapening (As)  vanaf drukrand
            double d2  = D2;              // drukwapening (As2) vanaf drukrand
            double As  = this.As;
            double As2 = this.As2;

            double eyd = fyd / Es;        // rek bij vloeigrens staal
            int    n   = 20;              // tussenliggende punten per regime

            // Pivot punt voor volledig gedrukte doorsnede (EC2 fig. 6.1 geval 3)
            double yp = (1.0 - ec3 / ecu3) * h;

            // ================================================================
            // Grenspunten berekenen
            // ================================================================

            // Geval 1: εcu3 boven, εs = εyd (trekwapening juist vloeiend)
            double x1 = d * ecu3 / (ecu3 + eyd);

            // Geval 2: εcu3 boven, neutrale as op onderkant (x = h → εbot = 0)
            // Geval 3 eindpunt = beginpunt geval 4: εtop = εc3, εbot = εc3

            // ================================================================
            // Regime 0→1: van kleine x tot x1 (staal trekt sterker dan vloeigrens)
            // ================================================================
            double xStart = h / 200.0; // vermijd deling door nul bij x→0
            for (int i = 0; i <= n; i++)
            {
                double x      = xStart + (x1 - xStart) * i / n;
                double epsBot = -ecu3 * (h - x) / x;
                InteractiePunten.Add(Punt(ecu3, epsBot));
            }

            // ================================================================
            // Regime 1→2: van x1 tot x = h (staal gaat van vloeiend naar gedrukt)
            // ================================================================
            for (int i = 1; i <= n; i++)
            {
                double x      = x1 + (h - x1) * i / n;
                double epsBot = -ecu3 * (h - x) / x;
                InteractiePunten.Add(Punt(ecu3, epsBot));
            }

            // ================================================================
            // Regime 2→3: volledig gedrukt, pivot rotatie
            // εtop daalt εcu3 → εc3; εbot = εc3 - (εtop - εc3)·(h-yp)/yp
            // ================================================================
            for (int i = 1; i <= n; i++)
            {
                double t      = (double)i / n;
                double epsTop = ecu3 - (ecu3 - ec3) * t;
                double epsBot = ec3  - (epsTop - ec3) * (h - yp) / yp;
                InteractiePunten.Add(Punt(epsTop, epsBot));
            }

            // ================================================================
            // Geval 4: zuivere druk (N maximaal, M = 0)
            // ================================================================
            var (nDruk, _) = BerekenNMStripIntegratie(ec3, ec3, b, h, d, d2, As, As2, fcd, fyd, Es, ec3);
            InteractiePunten.Add((nDruk, 0.0));

            // ================================================================
            // N=0 punt via interpolatie; filter op N >= 0
            // ================================================================
            var sorted = InteractiePunten.OrderBy(p => p.N).ToList();
            var neg = sorted.LastOrDefault(p => p.N < 0);
            var pos = sorted.FirstOrDefault(p => p.N >= 0);
            if (neg != default && pos != default)
            {
                double f  = (0.0 - neg.N) / (pos.N - neg.N);
                double m0 = neg.M + f * (pos.M - neg.M);
                sorted.Add((0.0, m0));
            }

            InteractiePunten = [.. sorted.Where(p => p.N >= 0)
                                        .OrderBy(p => p.N)
                                        .DistinctBy(p => (Math.Round(p.N, 4), Math.Round(p.M, 4)))];

            // ================================================================
            // Lokale hulpfunctie
            // ================================================================
            (double N, double M) Punt(double epsTop, double epsBot) =>
                BerekenNMStripIntegratie(epsTop, epsBot, b, h, d, d2, As, As2, fcd, fyd, Es, ec3);
        }

        /// <summary>
        /// Universele strip-integratie voor alle gevallen (1 t/m 3).
        /// eps_top = rek bovenkant (drukrand), eps_bot = rek onderkant.
        /// Beton draagt alleen druk; bi-lineaire spanning-rekrelatie.
        /// Krachten positief = druk. Moment t.o.v. hart doorsnede in kNm.
        /// </summary>
        private static (double N, double M) BerekenNMStripIntegratie(
            double eps_top, double eps_bot, double b, double h, double d, double d2,
            double As, double As2, double fcd, double fyd, double Es, double ec3)
        {
            const int strips = 50;
            double Fc = 0.0, MFc = 0.0;
            double stripHoogte = h / strips;
            for (int j = 0; j < strips; j++)
            {
                double y   = (j + 0.5) * stripHoogte;
                double eps = eps_top + (eps_bot - eps_top) * y / h;
                // Beton draagt geen trek; bi-lineaire spanning-rekrelatie voor druk
                double sigma;
                if (eps <= 0)       sigma = 0;                          // trek: geen bijdrage
                else if (eps < ec3) sigma = fcd * eps / ec3;            // oplopend deel
                else                sigma = fcd;                        // plateau
                double dF = sigma * b * stripHoogte / 1000.0;
                Fc  += dF;
                MFc += dF * (h / 2.0 - y);
            }

            // Trekwapening (diepte d vanaf bovenkant)
            double eps_s   = eps_top + (eps_bot - eps_top) * d / h;
            double sigma_s = Math.Clamp(eps_s * Es, -fyd, fyd);
            double Fs = sigma_s * As / 1000.0;

            // Drukwapening (diepte d2 vanaf bovenkant)
            double eps_s2   = eps_top + (eps_bot - eps_top) * d2 / h;
            double sigma_s2 = Math.Clamp(eps_s2 * Es, -fyd, fyd);
            double Fs2 = sigma_s2 * As2 / 1000.0;

            double N = Fc + Fs + Fs2;
            double M = (MFc + Fs * (h / 2.0 - d) + Fs2 * (h / 2.0 - d2)) / 1000.0;

            return (N, Math.Abs(M));
        }

        /// <summary>
        /// Berekent de momentweerstand MRd voor een opgegeven normaalkracht NEd
        /// door interpolatie in de interactiecurve.
        /// </summary>
        private double BerekenMomentWeerstand(double nEd)
        {
            if (InteractiePunten.Count < 2)
                return 0;

            // Zoek twee punten die NEd omvatten
            var sorted = InteractiePunten.OrderBy(p => p.N).ToList();

            // Als NEd buiten bereik, gebruik rand
            if (nEd <= sorted.First().N)
                return sorted.First().M;
            if (nEd >= sorted.Last().N)
                return sorted.Last().M;

            // Lineaire interpolatie
            for (int i = 0; i < sorted.Count - 1; i++)
            {
                var p1 = sorted[i];
                var p2 = sorted[i + 1];

                if (nEd >= p1.N && nEd <= p2.N)
                {
                    // Interpoleer
                    double factor = (nEd - p1.N) / (p2.N - p1.N);
                    return p1.M + factor * (p2.M - p1.M);
                }
            }

            return 0;
        }
    }
}
