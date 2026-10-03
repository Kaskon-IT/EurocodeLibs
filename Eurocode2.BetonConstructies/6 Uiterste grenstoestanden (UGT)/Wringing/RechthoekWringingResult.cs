namespace Eurocode.BetonConstructies
{
    /// <summary>
    /// Uitkomst van <see cref="RechthoekWringingCalculator"/>. Maten in mm, momenten in kNm, krachten in kN,
    /// langswapening in mm², beugelwapening in mm²/m.
    /// </summary>
    public sealed record RechthoekWringingResult
    {
        public required double TEd { get; init; }
        public required double VEd { get; init; }
        public required double CotTheta { get; init; }
        public required double Fctd { get; init; }
        public required double Fcd { get; init; }
        public required double Fyd { get; init; }

        /// <summary>ν = 0,6·(1 − f_ck/250), 6.2.2 (6), ook als ν₁ in V_Rd,max.</summary>
        public required double Nu { get; init; }

        // Equivalente dunwandige doorsnede (6.3.2 (1))
        /// <summary>A/u.</summary>
        public required double TefBerekend { get; init; }
        public required double TefBoven { get; init; }
        public required double TefOnder { get; init; }
        public required double TefZijkant { get; init; }
        public double TefMin => Math.Min(TefBoven, Math.Min(TefOnder, TefZijkant));

        /// <summary>Lengte van de boven- en onderwand tussen de hartlijnen van de zijwanden.</summary>
        public required double ZBreedte { get; init; }

        /// <summary>Lengte van de zijwanden tussen de hartlijnen van boven- en onderwand.</summary>
        public required double ZHoogte { get; init; }
        public required double Ak { get; init; }
        public required double Uk { get; init; }

        // Weerstanden
        /// <summary>Scheurmoment door wringing: 2·A_k·t_ef,min·f_ctd.</summary>
        public required double TRdc { get; init; }

        /// <summary>(6.30) met de kleinste t_ef,i en α_cw = 1.</summary>
        public required double TRdMax { get; init; }

        /// <summary>(6.9) met b_w = b, z = 0,9·d en α_cw = 1.</summary>
        public required double VRdMax { get; init; }
        public required double VRdc { get; init; }

        /// <summary>(6.31): T_Ed/T_Rd,c + V_Ed/V_Rd,c; NaN als V_Rd,c niet is opgegeven.</summary>
        public required double UnityCheck631 { get; init; }

        /// <summary>(6.29): T_Ed/T_Rd,max + V_Ed/V_Rd,max (drukdiagonalen).</summary>
        public required double UnityCheck629 { get; init; }

        /// <summary>(6.31) voldaan: alleen minimale wapening (9.2.1.1, 9.2.2) nodig.</summary>
        public bool IsAlleenMinimaleWapening => !double.IsNaN(UnityCheck631) && UnityCheck631 <= 1.0;

        // Wapening
        /// <summary>Beugelwapening voor wringing per wand: T_Ed / (2·A_k·f_yd·cot θ).</summary>
        public required double AswTPerMeter { get; init; }

        /// <summary>(6.28): ΣA_sl = T_Ed·u_k·cot θ / (2·A_k·f_yd).</summary>
        public required double AslTotaal { get; init; }
        public double AslBoven => ZBreedte / Uk * AslTotaal;
        public double AslOnder => ZBreedte / Uk * AslTotaal;

        /// <summary>Langswapening voor wringing per zijkant.</summary>
        public double AslZijkant => ZHoogte / Uk * AslTotaal;

        /// <summary>Benodigde wringwapening: 0 als (6.31) is voldaan.</summary>
        public double AswTBenodigdPerMeter => IsAlleenMinimaleWapening ? 0 : AswTPerMeter;
        public double AslBovenBenodigd => IsAlleenMinimaleWapening ? 0 : AslBoven;
        public double AslOnderBenodigd => IsAlleenMinimaleWapening ? 0 : AslOnder;
        public double AslZijkantBenodigd => IsAlleenMinimaleWapening ? 0 : AslZijkant;

        /// <summary>Dwarskracht + wringing, alle sneden: max(A_sw,V + 2·A_sw,T ; A_sw,min).</summary>
        public required double AswTotaalPerMeter { get; init; }

        /// <summary>Per snede: max(A_sw,tot / n ; A_sw,T) — de buitenste snede draagt de wringing.</summary>
        public required double AswPerSnedePerMeter { get; init; }
        public required int BeugelSneden { get; init; }

        /// <summary>9.2.3 (3): s ≤ u/8 en s ≤ min(b; h) (daarnaast s_l,max uit 9.2.2 (6)).</summary>
        public required double BeugelAfstandMax { get; init; }

        public required IReadOnlyList<string> Meldingen { get; init; }
    }
}
