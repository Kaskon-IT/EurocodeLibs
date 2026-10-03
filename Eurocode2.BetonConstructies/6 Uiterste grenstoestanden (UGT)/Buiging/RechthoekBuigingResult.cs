namespace Eurocode.BetonConstructies
{
    /// <summary>
    /// Uitkomst van <see cref="RechthoekBuigingCalculator"/>. Maten in mm, momenten in kNm,
    /// wapening in mm², spanningen in N/mm², rekken als getal (0,0035 = 3,5 ‰).
    /// </summary>
    public sealed record RechthoekBuigingResult
    {
        // Materiaal en drukzone
        public required double Alpha { get; init; }
        public required double Beta { get; init; }
        public required double EpsilonCu { get; init; }
        public required double Fcd { get; init; }
        public required double Fyd { get; init; }

        // Ontwerp: benodigde wapening voor MEd
        public required double MEd { get; init; }

        /// <summary>Drukzonehoogte zonder drukwapening; <see cref="double.NaN"/> als MEd zonder drukwapening niet opneembaar is.</summary>
        public required double Xu { get; init; }
        public required double XuMax { get; init; }

        /// <summary>Hefboomsarm bij het ontwerp (bij drukwapening: bij <i>x</i>u,max).</summary>
        public required double Z { get; init; }

        /// <summary>Grootste moment zonder drukwapening (bij <i>x</i>u,max).</summary>
        public required double MRdMaxZonderDrukwapening { get; init; }

        public required bool IsDrukwapeningNodig { get; init; }

        /// <summary>Rek en spanning in de drukwapening bij <i>x</i>u,max (0 zonder drukwapening).</summary>
        public required double EpsilonS2 { get; init; }
        public required double SigmaS2 { get; init; }

        /// <summary>Berekende trekwapening uit evenwicht, zonder minimum.</summary>
        public required double AsBerekend { get; init; }

        /// <summary>Minimale wapening: min(<i>A</i>s,min1 uit het scheurmoment; 1,25 <i>A</i>s,ber).</summary>
        public required double AsMin1 { get; init; }
        public required double AsMin { get; init; }
        public required double AsTrekBenodigd { get; init; }
        public required double AsDrukBenodigd { get; init; }
        public bool IsMinimaleWapeningMaatgevend => AsMin > AsBerekend;

        // Controle: momentcapaciteit van de aanwezige wapening uit evenwicht
        public required double AsTrekToegepast { get; init; }
        public required double AsDrukToegepast { get; init; }

        /// <summary>Drukzonehoogte bij <i>M</i>Rd (0 als er geen trekwapening is).</summary>
        public required double XMRd { get; init; }
        public required double SigmaS1MRd { get; init; }
        public required double SigmaS2MRd { get; init; }
        public required double MRd { get; init; }

        /// <summary><i>M</i>Ed / <i>M</i>Rd.</summary>
        public double UnityCheck => MRd > 0 ? MEd / MRd : MEd > 0 ? double.PositiveInfinity : 0;

        /// <summary>Drukzone bij <i>M</i>Rd groter dan <i>x</i>u,max: onvoldoende vervormingscapaciteit (6.1 (9)).</summary>
        public bool IsXMRdGroterDanXuMax => XMRd > XuMax;

        public required IReadOnlyList<string> Meldingen { get; init; }
    }
}
