namespace CommonLibrary.Modelling
{
    /// <summary> Eén gegenereerde staaf uit een <see cref="WapeningGroep"/>. </summary>
    public record StaafInstantie(StaafShape Shape, double Diameter);

    /// <summary>
    /// Wapeningsgroep (analoog aan Tekla RebarGroup): 1 of 2 staafvormen die
    /// langs een rechte verdeellijn (<see cref="VerdeelStart"/> → <see cref="VerdeelEind"/>)
    /// worden verdeeld volgens <see cref="Verdeling"/>.
    /// Geometrie zonder offsets (v1).
    /// </summary>
    public class WapeningGroep : BaseWapeningGroep
    {
        /// <summary>
        /// Te gebruiken voor bijvoorbeeld 'hs' voor haarspelden of 'bgl' voor beugels.
        /// </summary>
        public string Prefix { get; set; } = "";
        

        /// <summary> Weergavekleur (hex, bijv. "#ff8800"); null = standaardmateriaal. </summary>
        public string? Kleur { get; set; }

        /// <summary>
        /// Door de gebruiker opgegeven buigstralen [mm] (lengte 1 = voor alle knikken).
        /// Null of leeg = automatisch bepaald door de builder.
        /// Dit is de binnenste buigstraal r_i ofwel Øm/2.
        /// </summary>
        public double[]? Buigstralen { get; set; }

        /// <summary>
        /// Posities (t = 0..1) van de shapes langs de verdeellijn, één per shape.
        /// Tussen twee opeenvolgende posities wordt lineair geïnterpoleerd;
        /// zijn twee opeenvolgende shapes gelijk, dan blijft de vorm constant op dat traject.
        /// Null of leeg = gelijkmatig verdeeld (bijv. [0, 1] bij 2 shapes, [0, 0.5, 1] bij 3).
        /// </summary>
        public double[]? ShapePosities { get; set; }

        /// <summary> Startpunt van de verdeellijn (positie van de startvorm). </summary>
        public Punt3D VerdeelStart { get; set; }

        /// <summary>
        /// Eindpunt van de verdeellijn. Bij <see cref="VerdelingType.ExacteHartOpHart"/>
        /// wordt dit punt alleen als richting gebruikt en volgt de werkelijke lengte
        /// uit de som van de opgegeven afstanden.
        /// </summary>
        public Punt3D VerdeelEind { get; set; }

        public WapeningVerdeling Verdeling { get; set; } = new();

        /// <summary> Lengte van de verdeellijn L [mm] (bij ExacteHartOpHart: Σsᵢ), na aftrek van de offsets. </summary>
        public double VerdeelLengte => Verdeling.Type == VerdelingType.ExacteHartOpHart
            ? Verdeling.HartOpHartAfstanden.Sum()
            : EffectieveLengte;

        /// <summary> Beschikbare verdeellengte [mm]: afstand start→eind minus OffsetStart en OffsetEind. </summary>
        private double EffectieveLengte =>
            Math.Max(Punt3D.Afstand(VerdeelStart, VerdeelEind) - Verdeling.OffsetStart - Verdeling.OffsetEind, 0);

        /// <summary> Aantal staafposities, afhankelijk van het verdelingstype. </summary>
        public int AantalPosities => Verdeling.Type switch
        {
            VerdelingType.Gelijkmatig => Verdeling.Aantal,
            VerdelingType.ExacteHartOpHart => Verdeling.HartOpHartAfstanden.Count + 1,
            VerdelingType.BeoogdeHartOpHart => Verdeling.BeoogdeHartOpHart > 0
                ? (int)Math.Ceiling(EffectieveLengte / Verdeling.BeoogdeHartOpHart) + 1
                : 0,
            _ => 0,
        };

        /// <summary> Totaal aantal staven (posities × bundelgrootte). </summary>
        public int TotaalAantalStaven => AantalPosities;

        /// <summary> Totale wapeningsdoorsnede [mm²]. </summary>
        public double TotaalAs => TotaalAantalStaven * As;

        /// <summary> 
        /// Werkelijke h.o.h.-maat bij gelijkmatige verdeling [mm] 
        /// (0 bij 1 positie). 
        /// Kleinste hoh-maat bij exacte opgave
        /// </summary>
        public double WerkelijkeHartOpHart
        {
            get
            {
                switch (Verdeling.Type)
                {
                    case VerdelingType.ExacteHartOpHart:

                        if (Verdeling.HartOpHartAfstanden.Count == 0) return 0;
                        return Verdeling.HartOpHartAfstanden.Min(x=>x);
                    case VerdelingType.BeoogdeHartOpHart:
                        return Verdeling.BeoogdeHartOpHart;
                    case VerdelingType.Gelijkmatig:
                        if (AantalPosities > 1)
                        {
                            return VerdeelLengte / (AantalPosities - 1);
                        }
                        else
                        {
                            return 0;
                        }
                    default: return 0;
                }
            }
        }

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
            var start = VerdeelStart + Verdeling.OffsetStart * richting;
            var totaleLengte = Punt3D.Afstand(VerdeelStart, VerdeelEind);

            for (var i = 0; i < n; i++)
            {
                var afstand = Verdeling.Type == VerdelingType.ExacteHartOpHart
                    ? Verdeling.HartOpHartAfstanden.Take(i).Sum()
                    : n > 1 ? i * lengte / (n - 1) : 0;

                // interpolatiefactor t over de volledige verdeellijn (start→eind),
                // zodat de vorm bij meerdere shapes ook met offsets correct meeloopt
                var t = totaleLengte > 0 ? (Verdeling.OffsetStart + afstand) / totaleLengte : 0;

                yield return new StaafInstantie(
                    ShapeOpPositie(t).Transleer(start + afstand * richting),
                    Diameter);
            }
        }

        /// <summary>
        /// De (geïnterpoleerde) staafvorm op positie t (0..1) langs de verdeellijn.
        /// De shapes fungeren als keyframes op <see cref="ShapePosities"/>
        /// (of gelijkmatig verdeeld); tussen twee keyframes wordt puntsgewijs
        /// lineair geïnterpoleerd. Zo geeft bijv. [recht, recht, taps] met posities
        /// [0, 0.4, 1] een constante vorm tot t = 0.4 en daarna een verlopende vorm.
        /// </summary>
        public StaafShape ShapeOpPositie(double t)
        {
            if (Shapes.Length == 1) return Shapes[0];

            var posities = ShapePosities is { Length: > 0 } && ShapePosities.Length == Shapes.Length
                ? ShapePosities
                : [.. Enumerable.Range(0, Shapes.Length).Select(k => (double)k / (Shapes.Length - 1))];

            if (t <= posities[0]) return Shapes[0];
            if (t >= posities[^1]) return Shapes[^1];

            for (var k = 1; k < posities.Length; k++)
            {
                if (t > posities[k]) continue;
                var segment = posities[k] - posities[k - 1];
                var f = segment > 0 ? (t - posities[k - 1]) / segment : 1;
                return StaafShape.Lerp(Shapes[k - 1], Shapes[k], f);
            }

            return Shapes[^1];
        }

        public override void Valideer()
        {
            base.Valideer();
            Verdeling.Valideer();

            

            if (ShapePosities is { Length: > 0 })
            {
                if (ShapePosities.Length != Shapes.Length)
                {
                    throw new InvalidOperationException("ShapePosities moet evenveel waarden hebben als Shapes.");
                }

                if (ShapePosities.Zip(ShapePosities.Skip(1)).Any(p => p.Second < p.First))
                {
                    throw new InvalidOperationException("ShapePosities moet oplopend zijn.");
                }
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
