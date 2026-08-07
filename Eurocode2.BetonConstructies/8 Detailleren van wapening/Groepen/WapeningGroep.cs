namespace Eurocode.BetonConstructies
{
    /// <summary> Eén gegenereerde staaf uit een <see cref="WapeningGroep"/>. </summary>
    public record StaafInstantie(StaafShape Shape, double Diameter);

    /// <summary>
    /// Wapeningsgroep (analoog aan Tekla RebarGroup): 1 of 2 staafvormen die
    /// langs een rechte verdeellijn (<see cref="VerdeelStart"/> → <see cref="VerdeelEind"/>)
    /// worden verdeeld volgens <see cref="Verdeling"/>.
    /// Geometrie zonder offsets (v1).
    /// </summary>
    public class WapeningGroep : BaseWapening
    {
        /// <summary> Bundelgrootte n per positie (1 = geen bundel), §8.9. </summary>
        public int AantalStavenPerPositie { get; set; } = 1;

        /// <summary> Startpunt van de verdeellijn (positie van de startvorm). </summary>
        public Punt3D VerdeelStart { get; set; }

        /// <summary>
        /// Eindpunt van de verdeellijn. Bij <see cref="VerdelingType.ExacteHartOpHart"/>
        /// wordt dit punt alleen als richting gebruikt en volgt de werkelijke lengte
        /// uit de som van de opgegeven afstanden.
        /// </summary>
        public Punt3D VerdeelEind { get; set; }

        public WapeningVerdeling Verdeling { get; set; } = new();

        /// <summary> Lengte van de verdeellijn L [mm] (bij ExacteHartOpHart: Σsᵢ). </summary>
        public double VerdeelLengte => Verdeling.Type == VerdelingType.ExacteHartOpHart
            ? Verdeling.HartOpHartAfstanden.Sum()
            : Punt3D.Afstand(VerdeelStart, VerdeelEind);

        /// <summary> Aantal staafposities, afhankelijk van het verdelingstype. </summary>
        public int AantalPosities => Verdeling.Type switch
        {
            VerdelingType.Gelijkmatig => Verdeling.Aantal,
            VerdelingType.ExacteHartOpHart => Verdeling.HartOpHartAfstanden.Count + 1,
            VerdelingType.BeoogdeHartOpHart => Verdeling.BeoogdeHartOpHart > 0
                ? (int)Math.Ceiling(Punt3D.Afstand(VerdeelStart, VerdeelEind) / Verdeling.BeoogdeHartOpHart) + 1
                : 0,
            _ => 0,
        };

        /// <summary> Totaal aantal staven (posities × bundelgrootte). </summary>
        public int TotaalAantalStaven => AantalPosities * AantalStavenPerPositie;

        /// <summary> Totale wapeningsdoorsnede [mm²]. </summary>
        public double TotaalAs => TotaalAantalStaven * As;

        /// <summary> Equivalente diameter Øn = Ø·√n ≤ 55 mm bij bundels (§8.9.1). </summary>
        public double EquivalenteDiameter => AantalStavenPerPositie > 1
            ? Math.Min(Diameter * Math.Sqrt(AantalStavenPerPositie), 55)
            : Diameter;

        /// <summary> Werkelijke h.o.h.-maat bij gelijkmatige verdeling [mm] (0 bij 1 positie). </summary>
        public double WerkelijkeHartOpHart => AantalPosities > 1
            ? VerdeelLengte / (AantalPosities - 1)
            : 0;

        /// <summary>
        /// Minimale vrije tussenruimte tussen staven volgens §8.2:
        /// max(k1·Ø; dg + k2; 20 mm) met k1 = 1, k2 = 5 mm (NL-bijlage).
        /// </summary>
        public double MinimaleTussenruimte(double dg) =>
            Math.Max(Math.Max(Diameter, dg + 5), 20);

        /// <summary> Vrije tussenruimte tussen de staafposities [mm]. </summary>
        public double Tussenruimte => WerkelijkeHartOpHart > 0
            ? WerkelijkeHartOpHart - EquivalenteDiameter
            : double.PositiveInfinity;

        public bool TussenruimteVoldoet(double dg) => Tussenruimte >= MinimaleTussenruimte(dg);

        /// <summary>
        /// Genereert per staafpositie de (geïnterpoleerde en getransleerde) staaf.
        /// Bij 2 shapes wordt puntsgewijs lineair geïnterpoleerd tussen start- en eindvorm.
        /// </summary>
        public IEnumerable<StaafInstantie> GenereerStaven()
        {
            Valideer();

            var n = AantalPosities;
            var richting = (VerdeelEind - VerdeelStart).Genormaliseerd();
            var lengte = VerdeelLengte;

            for (var i = 0; i < n; i++)
            {
                var afstand = Verdeling.Type == VerdelingType.ExacteHartOpHart
                    ? Verdeling.HartOpHartAfstanden.Take(i).Sum()
                    : n > 1 ? i * lengte / (n - 1) : 0;

                var t = lengte > 0 ? afstand / lengte : 0;

                var shape = Shapes.Length == 2
                    ? StaafShape.Lerp(Shapes[0], Shapes[1], t)
                    : Shapes[0];

                yield return new StaafInstantie(
                    shape.Transleer(VerdeelStart + afstand * richting),
                    Diameter);
            }
        }

        public override void Valideer()
        {
            base.Valideer();
            Verdeling.Valideer();

            if (AantalStavenPerPositie < 1)
            {
                throw new InvalidOperationException("AantalStavenPerPositie moet ≥ 1 zijn.");
            }

            if (Verdeling.Type != VerdelingType.ExacteHartOpHart
                && AantalPosities > 1
                && Punt3D.Afstand(VerdeelStart, VerdeelEind) <= 0)
            {
                throw new InvalidOperationException("VerdeelStart en VerdeelEind mogen niet samenvallen bij meerdere staven.");
            }
        }
    }
}
