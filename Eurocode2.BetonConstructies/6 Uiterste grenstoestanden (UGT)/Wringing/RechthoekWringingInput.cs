namespace Eurocode.BetonConstructies
{
    /// <summary>
    /// Invoer voor wringing (6.3.2) van een massieve rechthoekige balk, eventueel gecombineerd met dwarskracht.
    /// Maten in mm, wringend moment in kNm, krachten in kN, beugelwapening in mm²/m.
    /// </summary>
    public sealed record RechthoekWringingInput
    {
        public required BetonContext Beton { get; init; }
        public required double Breedte { get; init; }
        public required double Hoogte { get; init; }

        /// <summary>Dekking op de beugel aan de boven-, onder- en zijkant.</summary>
        public required double DekkingBoven { get; init; }
        public required double DekkingOnder { get; init; }
        public required double DekkingZijkant { get; init; }

        public required double BeugelDiameter { get; init; }

        /// <summary>Diameter van de langsstaven aan elke zijde (voor de ondergrens van t_ef,i).</summary>
        public required double DiameterBoven { get; init; }
        public required double DiameterOnder { get; init; }
        public required double DiameterZijkant { get; init; }

        /// <summary>Rekenwaarde van het wringend moment; het teken wordt genegeerd.</summary>
        public double TEd { get; init; }

        /// <summary>Rekenwaarde van de dwarskracht; het teken wordt genegeerd.</summary>
        public double VEd { get; init; }

        /// <summary>Nuttige hoogte voor V_Rd,max (z = 0,9·d).</summary>
        public required double NuttigeHoogte { get; init; }

        /// <summary>V_Rd,c volgens (6.2) voor de combinatietoets (6.31); 0 = toets overslaan.</summary>
        public double VRdc { get; init; }

        /// <summary>cot θ van de drukdiagonalen; gelijk aan die van de dwarskrachtberekening (1 ≤ cot θ ≤ 2,5).</summary>
        public double CotTheta { get; init; } = 2.5;

        /// <summary>Benodigde dwarskrachtwapening (alle sneden samen, zonder minimum), voor de combinatie.</summary>
        public double AswVPerMeter { get; init; }

        /// <summary>Minimale dwarskrachtwapening (9.2.2 (5)), alle sneden samen.</summary>
        public double AswMinPerMeter { get; init; }

        /// <summary>Aantal beugelsneden; de buitenste twee nemen de wringing op.</summary>
        public int BeugelSneden { get; init; } = 2;
    }
}
