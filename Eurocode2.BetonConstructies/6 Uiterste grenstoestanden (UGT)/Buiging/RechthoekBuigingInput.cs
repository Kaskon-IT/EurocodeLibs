namespace Eurocode.BetonConstructies
{
    /// <summary>
    /// Invoer voor buiging van een rechthoekige doorsnede zonder normaalkracht (6.1).
    /// Maten in mm, moment in kNm, wapening in mm².
    /// </summary>
    public sealed record RechthoekBuigingInput
    {
        /// <summary>Beton en betonstaal; het spanning-rekdiagram van het beton bepaalt α en β.</summary>
        public required BetonContext Beton { get; init; }

        /// <summary>Breedte <i>b</i>.</summary>
        public required double Breedte { get; init; }

        /// <summary>Totale hoogte <i>h</i>.</summary>
        public required double Hoogte { get; init; }

        /// <summary>Nuttige hoogte <i>d</i> van de trekwapening.</summary>
        public required double NuttigeHoogte { get; init; }

        /// <summary>Afstand <i>d</i>₂ van het zwaartepunt van de drukwapening tot de gedrukte rand.</summary>
        public double AfstandDrukwapening { get; init; }

        /// <summary>Rekenwaarde van het moment; het teken wordt genegeerd.</summary>
        public double MEd { get; init; }

        /// <summary>Aanwezige trekwapening <i>A</i>ₛ₁ (voor <i>M</i>Rd).</summary>
        public double AsTrekToegepast { get; init; }

        /// <summary>Aanwezige drukwapening <i>A</i>ₛ₂ (voor <i>M</i>Rd).</summary>
        public double AsDrukToegepast { get; init; }
    }
}
