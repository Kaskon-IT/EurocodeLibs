using CommonLibrary;
using Eurocode.BetonConstructies.SpecifiekeRegels;
using Eurocode.Grondslagen;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Eurocode.BetonConstructies
{
    /// <summary>
    /// Berekening van een rechthoekige betonkolom (geschoorde kolom).
    /// EC2 §5.8 (2e orde), §6.1 (N-M interactie), §9.5.2 (minimale wapening).
    /// </summary>
    public class BetonKolomContext : BaseEurocodeContext
    {
        public override string Heading { get; set; } = "Betonkolom";

        public override void Init()
        {
            base.Init();
            WapeningVoorstellen = [];
        }

        // ================================================================
        // Input Contexten
        // ================================================================

        public BetonContext Beton { get; set; } = new(BetonsterkteklasseEnum.C45_55);

        /// <summary>
        /// Grondslagen inclusief nationale bijlage (EU, NL, BE, DE).
        /// Bepaalt nationale bijlage-waarden voor §9.5.2.
        /// </summary>
        public GrondslagenContext Grondslagen { get; set; } = new() { NationaleBijlage = NationaleBijlageEnum.NL };

        private NationaleBijlageEnum? NB => Grondslagen.NationaleBijlage;

        // ================================================================
        // Input Properties
        // ================================================================

        private double _breedte = 250;
        private double _hoogte  = 300;
        private double _lengte  = 4000;
        private double _dekking = 30;
        private double _beugelDiameter = 8;

        [TableColumn("Breedte doorsnede", Symbol = "<i>b</i>", Unit = "mm")]
        public double Breedte
        {
            get => _breedte;
            set => SetAndRecalculate(ref _breedte, value);
        }

        [TableColumn("Hoogte doorsnede", Symbol = "<i>h</i>", Unit = "mm")]
        public double Hoogte
        {
            get => HoogteGelijkAanBreedte ? _breedte : _hoogte;
            set => SetAndRecalculate(ref _hoogte, value);
        }

        [TableColumn("Kolomlengte (hart-hart oplegging)", Symbol = "<i>L</i>", Unit = "mm")]
        public double Lengte
        {
            get => _lengte;
            set => SetAndRecalculate(ref _lengte, value);
        }

        [TableColumn("Nominale dekking", Symbol = "<i>c</i><sub>nom</sub>", Unit = "mm")]
        public double Dekking
        {
            get => _dekking;
            set => SetAndRecalculate(ref _dekking, value);
        }

        [TableColumn("Beugeldiameter", Symbol = "<i>Ø</i><sub>bgl</sub>", Unit = "mm")]
        public double BeugelDiameter
        {
            get => _beugelDiameter;
            set => SetAndRecalculate(ref _beugelDiameter, value);
        }

        /// <summary>Bij Vierkant of Cirkel is H gelijk aan B (invoer H disabled).</summary>
        public bool HoogteGelijkAanBreedte => KolomVorm is KolomVormEnum.Vierkant or KolomVormEnum.Cirkel;

        /// <summary>Bij Wand zijn KolomVorm en ExcentriciteitType geforceerd (invoer disabled).</summary>
        public bool IsWand => KolomType == KolomTypeEnum.Wand;

        private double _nEd = 750;

        [TableColumn("Rekenwaarde normaalkracht", Symbol = "<i>N</i><sub>Ed</sub>", Unit = "kN")]
        public double NEd
        {
            get => _nEd;
            set => SetAndRecalculate(ref _nEd, value);
        }

        private double _phiEf = 0.0;

        [TableColumn("Effectieve kruipverhouding", Symbol = "<i>φ</i><sub>ef</sub>", Unit = "-")]
        public double PhiEf
        {
            get => _phiEf;
            set => SetAndRecalculate(ref _phiEf, value);
        }

        // Wapening invoer: opgegeven staven of direct As
        private bool   _waperningAlsStaven = true;
        private int    _aantalStaven = 0;          // 0 = niet opgegeven (berekening bepaalt)
        private double _diameterLangs = 16.0;
        private double _asOpgegeven = 0.0;

        /// <summary>True = invoer via n staven Ød; False = invoer via As direct.</summary>
        public bool WapeningAlsStaven
        {
            get => _waperningAlsStaven;
            set => SetAndRecalculate(ref _waperningAlsStaven, value);
        }

        /// <summary>Aantal langsstaven (0 = automatisch bepalen).</summary>
        public int AantalStaven
        {
            get => _aantalStaven;
            set => SetAndRecalculate(ref _aantalStaven, value);
        }

        /// <summary>Diameter langsstaven [mm].</summary>
        public double DiameterLangs
        {
            get => _diameterLangs;
            set => SetAndRecalculate(ref _diameterLangs, Math.Max(6.0, value));
        }

        /// <summary>Opgegeven As direct [mm²] (alleen actief als WapeningAlsStaven = false).</summary>
        public double AsOpgegeven
        {
            get => _asOpgegeven;
            set => SetAndRecalculate(ref _asOpgegeven, Math.Max(0.0, value));
        }

        /// <summary>Berekende As op basis van opgegeven staven [mm²], of AsOpgegeven.</summary>
        public double AsInvoer =>
            WapeningAlsStaven && AantalStaven > 0
                ? AantalStaven * Math.PI * DiameterLangs * DiameterLangs / 4.0
                : (!WapeningAlsStaven && AsOpgegeven > 0 ? AsOpgegeven : 0.0);

        // Excentriciteitstype
        private ExcentriciteitTypeEnum _excentriciteitType = ExcentriciteitTypeEnum.Dubbel;

        public ExcentriciteitTypeEnum ExcentriciteitType
        {
            get => IsWand ? ExcentriciteitTypeEnum.GeschoordUitHetVlak : _excentriciteitType;
            set => SetAndRecalculate(ref _excentriciteitType, value);
        }

        private KolomBelastingSchemaEnum _belastingSchema = KolomBelastingSchemaEnum.Geschoord;
        public KolomBelastingSchemaEnum BelastingSchema
        {
            get => _belastingSchema;
            set => SetAndRecalculate(ref _belastingSchema, value);
        }

        private KolomTypeEnum _kolomType = KolomTypeEnum.Kolom;
        public KolomTypeEnum KolomType
        {
            get => _kolomType;
            set => SetAndRecalculate(ref _kolomType, value);
        }   

        private KolomVormEnum _kolomVorm = KolomVormEnum.Vierkant;
        public KolomVormEnum KolomVorm
        {
            get => IsWand ? KolomVormEnum.Rechthoek : _kolomVorm;
            set => SetAndRecalculate(ref _kolomVorm, value);
        }


        public bool IsDubbel => ExcentriciteitType == ExcentriciteitTypeEnum.Dubbel;
        public bool IsEnkel => ExcentriciteitType == ExcentriciteitTypeEnum.GeschoordUitHetVlak;

        // Kniklengtefactor per richting (β-waarde direct, l₀ = β · L)
        private double _kniklengtefactorX = 1.0;
        private double _kniklengtefactorY = 1.0;

        [TableColumn("Kniklengtefactor X-as", Symbol = "<i>β</i><sub>x</sub>", Unit = "-")]
        public double KniklengtefactorX
        {
            get => _kniklengtefactorX;
            set => SetAndRecalculate(ref _kniklengtefactorX, Math.Max(0.1, value));
        }

        [TableColumn("Kniklengtefactor Y-as", Symbol = "<i>β</i><sub>y</sub>", Unit = "-")]
        public double KniklengtefactorY
        {
            get => _kniklengtefactorY;
            set => SetAndRecalculate(ref _kniklengtefactorY, Math.Max(0.1, value));
        }

        /// <summary>Kniklengte X-as: l₀ = β · L [mm].</summary>
        public double L0X => KniklengtefactorX * Lengte;

        /// <summary>Kniklengte Y-as: l₀ = β · L [mm].</summary>
        public double L0Y => KniklengtefactorY * Lengte;

        // Krommingsverdeling factor c per richting (standaard 10, π² ≈ 9.87 voor sinus)
        private double _cX = 10.0;
        private double _cY = 10.0;

        [TableColumn("Krommingsverd. factor c (X-as)", Symbol = "<i>c</i><sub>x</sub>", Unit = "-")]
        public double CX
        {
            get => _cX;
            set => SetAndRecalculate(ref _cX, Math.Max(1.0, value));
        }

        [TableColumn("Krommingsverd. factor c (Y-as)", Symbol = "<i>c</i><sub>y</sub>", Unit = "-")]
        public double CY
        {
            get => _cY;
            set => SetAndRecalculate(ref _cY, Math.Max(1.0, value));
        }

        // Eerste-orde momenten X-richting (buiging om X-as, moment in vlak van Hoogte h)
        private double _m01X = 0.0;
        private double _m02X = 0.0;

        [TableColumn("1e orde moment M01 (X-as)", Symbol = "<i>M</i><sub>01,x</sub>", Unit = "kNm")]
        public double M01X
        {
            get => _m01X;
            set => SetAndRecalculate(ref _m01X, value);
        }

        [TableColumn("1e orde moment M02 (X-as)", Symbol = "<i>M</i><sub>02,x</sub>", Unit = "kNm")]
        public double M02X
        {
            get => _m02X;
            set => SetAndRecalculate(ref _m02X, value);
        }

        // Eerste-orde momenten Y-richting (buiging om Y-as, moment in vlak van Breedte b)
        private double _m01Y = 0.0;
        private double _m02Y = 0.0;

        [TableColumn("1e orde moment M01 (Y-as)", Symbol = "<i>M</i><sub>01,y</sub>", Unit = "kNm")]
        public double M01Y
        {
            get => _m01Y;
            set => SetAndRecalculate(ref _m01Y, value);
        }

        [TableColumn("1e orde moment M02 (Y-as)", Symbol = "<i>M</i><sub>02,y</sub>", Unit = "kNm")]
        public double M02Y
        {
            get => _m02Y;
            set => SetAndRecalculate(ref _m02Y, value);
        }

        // ================================================================
        // Output Properties
        // ================================================================

        /// <summary>
        /// Oppervlak doorsnede in mm².
        /// </summary>
        public double Ac => Breedte * Hoogte;

        /// <summary>§9.5.2 (2) As,min = max(0.10·NEd/fyd ; 0.002·Ac) [mm²].</summary>
        public double AsMin952_2 => Kolommen.AsMin(NEd, Beton.BetonStaal.Fyd, Ac);

        /// <summary>§9.5.2 (1)+(4) As,min op basis van minimale diameter én minimaal aantal staven [mm²].</summary>
        public double AsMin952_1_en_4 => Kolommen.AsMin952_1_en_4(NB);

        /// <summary>§9.5.2 (3) As,max = 0.04·Ac buiten lasnaden [mm²].</summary>
        public double AsMax952_3 => Kolommen.AsMax(Ac);

        /// <summary>§9.5.2 (3) As,max = 0.08·Ac ter plaatse van lasnaden [mm²].</summary>
        public double AsMax952_3Lasnade => Kolommen.AsMaxLasnade(Ac);

        /// <summary>§9.5.2 (1) Minimale diameter langswapening [mm] – nationale bijlage afhankelijk.</summary>
        public double DiameterMinLangs => Kolommen.DiameterMinLangs(NB);

        /// <summary>§9.5.2 (4) Minimaal aantal staven in rechthoekige doorsnede [-].</summary>
        public int AantalStavenMin => Kolommen.AantalStavenMinRechthoek;

        /// <summary>§9.5.3 (3) Minimale beugeldiameter [mm] – afhankelijk van gekozen langswapening.</summary>
        public double BeugelDiameterMin(double diameterLangs) => Kolommen.BeugelDiameterMin(diameterLangs);

        /// <summary>§9.5.3 (1) Maximale hartafstand beugels [mm].</summary>
        public double BeugelsHartAfstandMax(double diameterLangs) => Kolommen.BeugelsHartAfstandMax(diameterLangs, Breedte, Hoogte);

        /// <summary>
        /// Wapening berekend uit M-N interactie, vóór toepassing van minimale eisen [mm²].
        /// </summary>
        public double AsBerekend { get; private set; }

        /// <summary>
        /// Benodigde wapening As,req (grootste van minimale eisen en M-N berekening, maatgevende richting) [mm²].
        /// </summary>
        public double AsRequired { get; private set; }

        /// <summary>
        /// Maatgevende momentweerstand (laagste UC-richting).
        /// </summary>
        public double MRd { get; private set; }

        /// <summary>
        /// Maatgevende Unity Check (max van beide richtingen).
        /// </summary>
        public double UnityCheck { get; private set; }

        /// <summary>
        /// Lijst van wapeningsvoorstellen (nØd).
        /// </summary>
        public List<KolomWapeningVoorstel> WapeningVoorstellen { get; private set; } = [];

        /// <summary>
        /// Tussenresultaten van de laatste berekening (per artikel, beide richtingen).
        /// </summary>
        public KolomTussenresultaten? Tussenresultaten { get; private set; }

        // ================================================================
        // Berekening
        // ================================================================

        protected override void Bereken()
        {
            WapeningVoorstellen.Clear();

            double fyd = Beton.BetonStaal.Fyd;
            double fcd = Beton.Fcd;
            double fck = Beton.Fck;
            double es  = Beton.BetonStaal.ElasticiteitsModulus;
            double nEd = NEd;
            double ac  = Ac;
            double asMin = Math.Max(AsMin952_2, AsMin952_1_en_4);
            // Als gebruiker wapening heeft opgegeven, die als ondergrens gebruiken
            if (AsInvoer > 0)
                asMin = Math.Max(asMin, AsInvoer);

            // d2: hart langswapening t.o.v. rand
            double d2 = Dekking + BeugelDiameter + DiameterLangs / 2.0;

            // Bereken per richting
            var rx = BerekenRichting(Hoogte, Breedte, M01X, M02X, d2, nEd, ac, fcd, fck, fyd, es, asMin, L0X, CX);
            var ry = IsDubbel
                ? BerekenRichting(Breedte, Hoogte, M01Y, M02Y, d2, nEd, ac, fcd, fck, fyd, es, asMin, L0Y, CY)
                : null;

            // Maatgevende wapening = max van beide richtingen
            double asReqMax = ry is not null ? Math.Max(rx.AsReq, ry.AsReq) : rx.AsReq;

            // Definitieve MRd en UC: beide richtingen met maatgevende As
            double mRdX = BerekenMRd(asReqMax, nEd, fcd, fyd, Hoogte, Breedte, d2);
            double mRdY = ry is not null ? BerekenMRd(asReqMax, nEd, fcd, fyd, Breedte, Hoogte, d2) : 0.0;
            double ucX  = mRdX > 0 ? rx.MEdTot / mRdX : double.MaxValue;
            double ucY  = ry is not null && mRdY > 0 ? ry.MEdTot / mRdY : 0.0;

            // NRd = Ac·fcd + As·fyd  (§5.8.9)
            double nRd = (ac * fcd + asReqMax * fyd) / 1000.0;  // [kN]
            double ratio = nRd > 0 ? nEd / nRd : 1.0;
            double exponentA = ratio <= 0.1 ? 1.0
                             : ratio >= 1.0 ? 2.0
                             : ratio <= 0.7 ? 1.0 + (ratio - 0.1) / (0.7 - 0.1) * (1.5 - 1.0)
                             :                1.5 + (ratio - 0.7) / (1.0 - 0.7) * (2.0 - 1.5);
            double ucCombi = ry is not null
                ? Math.Pow(ucX, exponentA) + Math.Pow(ucY, exponentA)
                : ucX;

            AsBerekend = ry is not null ? Math.Max(rx.AsCalc, ry.AsCalc) : rx.AsCalc;
            AsRequired = asReqMax;
            MRd        = ucX >= ucY ? mRdX : mRdY;
            UnityCheck = ry is not null ? ucCombi : ucX;

            // Tussenresultaten
            Tussenresultaten = new KolomTussenresultaten
            {
                NEd        = nEd,
                L0_x       = L0X,
                L0_y       = IsDubbel ? L0Y : 0.0,
                Kruipfactor = PhiEf,
                I_x        = Breedte * Math.Pow(Hoogte, 3) / 12.0,
                I_y        = Hoogte  * Math.Pow(Breedte, 3) / 12.0,
                Ac         = ac,
                Fcd        = fcd,
                As         = asReqMax,
                Fyd        = fyd,
                Es         = es,
                RichtingX  = rx.Tussenresultaten,
                RichtingY  = ry?.Tussenresultaten ?? new(),
                MRd_x      = mRdX,
                MRd_y      = mRdY,
                NRd        = nRd,
                UC_x       = ucX,
                UC_y       = ucY,
                ExponentA  = exponentA,
                UC_combi   = ucCombi,
                UnityCheck = UnityCheck,
            };

            // Wapeningsvoorstellen: 4, 6, 8, 12 staven, diameters 10..32
            List<double> diameters = [10, 12, 16, 20, 25, 32];
            List<int> aantallen    = [4, 6, 8, 12];
            double mEdMax = ry is not null ? Math.Max(rx.MEdTot, ry.MEdTot) : rx.MEdTot;

            foreach (var n in aantallen)
            {
                foreach (var dia in diameters)
                {
                    double asStaaf = Math.PI * dia * dia / 4.0;
                    double asTotal = n * asStaaf;
                    if (asTotal >= asReqMax && dia >= DiameterMinLangs && asTotal <= AsMax952_3)
                    {
                        double mRdVoorstel = BerekenMRd(asTotal, nEd, fcd, fyd, Hoogte, Breedte, d2);
                        WapeningVoorstellen.Add(new KolomWapeningVoorstel(n, dia, asTotal, mRdVoorstel, mEdMax));
                        break; // kleinste geschikte diameter per aantal staven
                    }
                }
            }
        }

        protected override bool Valideer()
        {
            bool ok = UnityCheck <= 1.0;
            if (!ok)
                AddMeldingWaarschuwing($"Unity Check ({UnityCheck:F2}) > 1.0 – doorsnede of wapening onvoldoende.");
            if (AsRequired > AsMax952_3)
                AddMeldingWaarschuwing($"Benodigde wapening ({AsRequired:F0} mm²) overschrijdt As,max ({AsMax952_3:F0} mm²) – vergroot doorsnede.");
            return ok;
        }

        // ================================================================
        // Private helpers – richtingsberekening
        // ================================================================

        private record RichtingResultaat(
            double AsReq, double AsCalc, double MEdTot,
            KolomRichtingTussenresultaten Tussenresultaten);

        /// <summary>
        /// Berekent tweede-orde effecten en benodigde wapening voor één buigrichting.
        /// h_richting = afmeting in buigvlak (Hoogte voor X-as, Breedte voor Y-as).
        /// b_richting = breedte drukvlak (Breedte voor X-as, Hoogte voor Y-as).
        /// </summary>
        private RichtingResultaat BerekenRichting(
            double hRichting, double bRichting,
            double m01, double m02,
            double d2, double nEd, double ac,
            double fcd, double fck, double fyd, double es, double asMin,
            double l0, double c)
        {
            // EC2 §5.8.3.1: M02 is het grootste eindmoment (|M02| ≥ |M01|)
            if (Math.Abs(m01) > Math.Abs(m02))
                (m01, m02) = (m02, m01);

            double lambda = TweedeOrdeEffecten.Slankheid(l0, hRichting);
            double ei     = GeometrischeOnvolkomenheid.Ei(l0);
            double e0     = GeometrischeOnvolkomenheid.E0(l0, hRichting, NB);

            // §5.2(7): eindmomenten begrenzen op N·eᵢ (geometrische imperfectie-excentriciteit)
            double mMinEi = nEd * ei / 1000.0;
            if (Math.Abs(m01) < mMinEi) m01 = Math.Sign(m01) == 0 ? mMinEi : Math.Sign(m01) * mMinEi;
            if (Math.Abs(m02) < mMinEi) m02 = Math.Sign(m02) == 0 ? mMinEi : Math.Sign(m02) * mMinEi;

            // Na begrenzing opnieuw controleren dat |M02| ≥ |M01|
            if (Math.Abs(m01) > Math.Abs(m02))
                (m01, m02) = (m02, m01);

            // Momentopbouw: M0Ed = |M02| + NEd·e0 (geometrische onvolkomenheid toegevoegd aan 1e orde moment)
            double rm     = Math.Abs(m02) > 0 ? m01 / m02 : 0.0;
            double e2     = 0.0;
            double asReq  = asMin;

            for (int iter = 0; iter < 20; iter++)
            {
                double omega = asReq * fyd / (ac * fcd);
                double n     = nEd * 1000.0 / (ac * fcd);
                double lLim  = TweedeOrdeEffecten.SlankheidsGrens(n, omega, rm, PhiEf);

                e2 = lambda > lLim
                    ? TweedeOrdeEffecten.E2(nEd, asReq, l0, hRichting, d2, fcd, fck, fyd, es, ac, PhiEf, c)
                    : 0.0;

                // Totaal rekenmoment: M0Ed = |M02| (incl. imperfectie §5.2), MEd,min = N·e0 (§6.1(4))
                double mEd0    = Math.Abs(m02);
                double mEdMin  = nEd * e0 / 1000.0;
                double m0e     = Math.Max(0.6 * Math.Abs(m02) + 0.4 * Math.Abs(m01), 0.4 * Math.Abs(m02));
                double mEdTot  = Math.Max(Math.Max(m0e + nEd * e2 / 1000.0, Math.Abs(m01)), Math.Max(mEd0, mEdMin));

                double asCalc = 0.0;
                if (BerekenMRd(0.0, nEd, fcd, fyd, hRichting, bRichting, d2) < mEdTot)
                {
                    asCalc = 0.001 * ac;
                    double mRd = BerekenMRd(asCalc, nEd, fcd, fyd, hRichting, bRichting, d2);
                    int maxIt  = 60;
                    while (mRd < mEdTot && maxIt-- > 0)
                    {
                        asCalc *= 1.15;
                        mRd     = BerekenMRd(asCalc, nEd, fcd, fyd, hRichting, bRichting, d2);
                    }
                }

                double asNew = Math.Max(asCalc, asMin);
                if (Math.Abs(asNew - asReq) < 1.0) { asReq = asNew; break; }
                asReq = asNew;
            }

            // Definitief
            double omegaFin = asReq * fyd / (ac * fcd);
            double nFin     = nEd * 1000.0 / (ac * fcd);
            double lLimFin  = TweedeOrdeEffecten.SlankheidsGrens(nFin, omegaFin, rm, PhiEf);
            e2 = lambda > lLimFin
                ? TweedeOrdeEffecten.E2(nEd, asReq, l0, hRichting, d2, fcd, fck, fyd, es, ac, PhiEf, c)
                : 0.0;

            double mEd0Fin   = Math.Abs(m02);
            double mEdMinFin = nEd * e0 / 1000.0;
            // §5.8.8.2(2): equivalent 1e-orde moment M0e
            double m0eFin    = Math.Max(0.6 * Math.Abs(m02) + 0.4 * Math.Abs(m01), 0.4 * Math.Abs(m02));
            double m2kNm     = nEd * e2 / 1000.0;
            double mEdVeld   = m0eFin + m2kNm;
            // MEd,boven en MEd,onder: M0e ± M2/2 (parabolische krommingsverdeling §5.8.8.2)
            // teken van M02 bepaalt de richting
            double sign      = m02 >= 0 ? 1.0 : -1.0;
            double mEdBoven  = sign * (m0eFin - m2kNm / 2.0);
            double mEdOnder  = sign * (m0eFin - m2kNm / 2.0);
            double mEdTotFin = Math.Max(Math.Max(Math.Max(mEdVeld, Math.Abs(mEdBoven)), Math.Abs(mEdOnder)), mEdMinFin);

            double asCalcFin = 0.0;
            if (BerekenMRd(0.0, nEd, fcd, fyd, hRichting, bRichting, d2) < mEdTotFin)
            {
                asCalcFin = 0.001 * ac;
                double mRdFin = BerekenMRd(asCalcFin, nEd, fcd, fyd, hRichting, bRichting, d2);
                int maxIt2    = 60;
                while (mRdFin < mEdTotFin && maxIt2-- > 0)
                {
                    asCalcFin *= 1.15;
                    mRdFin     = BerekenMRd(asCalcFin, nEd, fcd, fyd, hRichting, bRichting, d2);
                }
            }

            // Tussenresultaten opbouw
            double kphi    = TweedeOrdeEffecten.Kphi(fck, lambda, PhiEf);
            double beta    = 0.35 + fck / 200.0 - lambda / 150.0;
            double d       = hRichting - d2;
            double kr0     = fyd / (es * 0.45 * d);
            double nuVal   = 1.0 + omegaFin;
            double kr      = Math.Clamp((nuVal - nFin) / (nuVal - 0.4), 0.0, 1.0);
            double alphaH  = GeometrischeOnvolkomenheid.AlphaH(l0);
            double alphaM  = GeometrischeOnvolkomenheid.AlphaM();
            double thetaI  = GeometrischeOnvolkomenheid.ThetaI(l0);
            double eiTr    = GeometrischeOnvolkomenheid.Ei(l0);

            var tr = new KolomRichtingTussenresultaten
            {
                L0               = l0,
                M01              = m01,
                M02              = m02,
                Rm               = rm,
                Lambda           = lambda,
                LambdaLim        = lLimFin,
                TweedeOrdeRelevant = lambda > lLimFin,
                FactorA          = PhiEf > 0.0 ? 1.0 / (1.0 + 0.2 * PhiEf) : 0.7,
                FactorB          = Math.Sqrt(1.0 + 2.0 * omegaFin),
                FactorC          = 0.7,
                Omega            = omegaFin,
                N_rel            = nFin,
                D                = d,
                Nu               = nuVal,
                Nbal             = 0.4,
                Kr               = kr,
                Beta             = beta,
                KphiVal          = kphi,
                Kromming0        = kr0,
                Kromming         = kr * kphi * kr0,
                C                = c,
                E2               = e2,
                M2               = nEd * e2 / 1000.0,
                E0               = e0,
                Theta0           = GeometrischeOnvolkomenheid.Theta0,
                AlphaH           = alphaH,
                AlphaM           = alphaM,
                ThetaI           = thetaI,
                Ei               = eiTr,
                MEd_min          = mEdMinFin,
                M0e              = m0eFin,
                MEd_boven        = mEdBoven,
                MEd_veld         = mEdVeld,
                MEd_onder        = mEdOnder,
                ETotaal          = e0 + e2,
                MEd_totaal       = mEdTotFin,
            };

            return new RichtingResultaat(Math.Max(asCalcFin, asMin), asCalcFin, mEdTotFin, tr);
        }

        // ================================================================
        // Private helpers – M-N berekening met rechthoekig spanningsblok
        // ================================================================

        /// <summary>
        /// Berekent MRd bij gegeven NEd en As (totale symmetrische wapening).
        /// h = afmeting in buigvlak, b = breedte drukvlak.
        /// </summary>
        private double BerekenMRd(double asTotal, double nEd, double fcd, double fyd,
                                   double h, double b, double d2)
        {
            double Es   = Beton.BetonStaal.ElasticiteitsModulus;
            double ecu3 = Beton.EpsilonCu3;
            double ec3  = Beton.EpsilonC3;
            double fck  = Beton.Fck;
            double lam  = fck <= 50 ? 0.8 : 0.8 - (fck - 50) / 400.0;
            double eta  = fck <= 50 ? 1.0 : 1.0 - (fck - 50) / 200.0;
            double d    = h - d2;

            double As  = asTotal / 2.0;
            double As2 = asTotal / 2.0;

            double xLow = 1.0, xHigh = h * 2.0;
            double x = h / 2.0;
            for (int i = 0; i < 60; i++)
            {
                x = (xLow + xHigh) / 2.0;
                double nCalc = BerekenN(x, b, h, d, d2, As, As2, fcd, fyd, Es, lam, eta, ecu3, ec3);
                if (nCalc < nEd) xLow  = x;
                else             xHigh = x;
            }

            double xBlok  = Math.Min(lam * x, h);
            double Fc_kN  = eta * fcd * xBlok * b / 1000.0;

            double eps_s   = ecu3 * (x - d)  / x;
            double sigma_s = Math.Clamp(eps_s * Es, -fyd, fyd);
            double Fs_kN   = sigma_s * As  / 1000.0;

            double eps_s2   = ecu3 * (x - d2) / x;
            double sigma_s2 = Math.Clamp(eps_s2 * Es, -fyd, fyd);
            double Fs2_kN   = sigma_s2 * As2 / 1000.0;

            double M = (Fc_kN  * (h / 2.0 - xBlok / 2.0)
                      + Fs_kN  * (h / 2.0 - d)
                      + Fs2_kN * (h / 2.0 - d2)) / 1000.0;

            return Math.Abs(M);
        }

        private static double BerekenN(
            double x, double b, double h, double d, double d2,
            double As, double As2, double fcd, double fyd, double Es,
            double lambda, double eta, double ecu3, double ec3)
        {
            double xBlok = Math.Min(lambda * x, h);
            double Fc    = eta * fcd * xBlok * b / 1000.0;

            double eps_s   = x > 0 ? ecu3 * (x - d)  / x : -fyd / Es;
            double sigma_s = Math.Clamp(eps_s * Es, -fyd, fyd);
            double Fs      = sigma_s * As  / 1000.0;

            double eps_s2   = x > 0 ? ecu3 * (x - d2) / x : -fyd / Es;
            double sigma_s2 = Math.Clamp(eps_s2 * Es, -fyd, fyd);
            double Fs2      = sigma_s2 * As2 / 1000.0;

            return Fc + Fs + Fs2;
        }
    }

    /// <summary>
    /// Wapeningsvoorstel voor een kolom: n staven met diameter d.
    /// </summary>
    public record KolomWapeningVoorstel(int Aantal, double Diameter, double AsProvided, double MRd, double MEd)
    {
        public string Tekst         => $"{Aantal}Ø{Diameter:0}";
        public double UnityCheck    => MRd > 0 ? MEd / MRd : double.MaxValue;
        public bool   Voldoet       => UnityCheck <= 1.0;
    }

    /// <summary>
    /// Tussenresultaten per buigrichting.
    /// </summary>
    public record KolomRichtingTussenresultaten
    {
        public double L0               { get; init; }
        public double M01              { get; init; }
        public double M02              { get; init; }
        public double Rm               { get; init; }
        public double Lambda           { get; init; }
        public double LambdaLim        { get; init; }
        public bool   TweedeOrdeRelevant { get; init; }
        public double FactorA          { get; init; }
        public double FactorB          { get; init; }
        public double FactorC          { get; init; }
        public double Omega            { get; init; }
        public double N_rel            { get; init; }
        public double D                { get; init; }
        public double Nu               { get; init; }
        public double Nbal             { get; init; }
        public double Kr               { get; init; }
        public double Beta             { get; init; }
        public double KphiVal          { get; init; }
        public double Kromming0        { get; init; }
        public double Kromming         { get; init; }
        public double C                { get; init; }
        public double E2               { get; init; }
        public double M2               { get; init; }
        public double E0               { get; init; }
        public double Theta0           { get; init; }
        public double AlphaH           { get; init; }
        public double AlphaM           { get; init; }
        public double ThetaI           { get; init; }
        public double Ei               { get; init; }
        public double MEd_min          { get; init; }
        public double M0e              { get; init; }
        public double MEd_boven        { get; init; }
        public double MEd_veld         { get; init; }
        public double MEd_onder        { get; init; }
        public double ETotaal          { get; init; }
        public double MEd_totaal       { get; init; }
    }

    /// <summary>
    /// Gecombineerde tussenresultaten voor X en Y richting.
    /// </summary>
    public record KolomTussenresultaten
    {
        // Algemeen
        public double NEd         { get; init; }
        public double L0_x        { get; init; }
        public double L0_y        { get; init; }
        public double Kruipfactor { get; init; }
        public double I_x         { get; init; }
        public double I_y         { get; init; }
        public double Ac          { get; init; }
        public double Fcd         { get; init; }
        public double As          { get; init; }
        public double Fyd         { get; init; }
        public double Es          { get; init; }

        // Per richting
        public KolomRichtingTussenresultaten RichtingX { get; init; } = new();
        public KolomRichtingTussenresultaten RichtingY { get; init; } = new();

        // Eindresultaten
        public double MRd_x       { get; init; }
        public double MRd_y       { get; init; }
        public double NRd         { get; init; }
        public double UC_x        { get; init; }
        public double UC_y        { get; init; }
        public double ExponentA   { get; init; }
        public double UC_combi    { get; init; }
        public double UnityCheck  { get; init; }
    }
}
