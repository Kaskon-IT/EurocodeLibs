namespace CommonLibrary.Modelling
{
    /// <summary>
    /// De vorm van een wapeningsstaaf: polyline (hartlijn) + buigstralen per knik.
    /// </summary>
    public class StaafShape
    {
        /// <summary> Hartlijnpunten van de staaf [mm]. Minimaal 2 punten. </summary>
        public List<Punt3D> Punten { get; set; } = [];

        /// <summary>
        /// Buigstralen (hartlijn) per knik, index = binnenpunt − 1 (knik 0 = punt 1).
        /// - lengte 1: één straal voor alle knikken (het gangbare geval, Tabel 8.1N)
        /// - lengte n−2: expliciet per knik (b.v. haarspeld: grote lus, kleine haken)
        /// </summary>
        public double[] Buigstralen { get; set; } = [];

        /// <summary> Aantal knikken (binnenpunten) van de polyline. </summary>
        public int AantalKnikken => Math.Max(0, Punten.Count - 2);

        public double BuigstraalBijKnik(int i) =>
            Buigstralen.Length == 1 ? Buigstralen[0] : Buigstralen[i];

        /// <summary> Rechte beenlengtes tussen de punten (A, B, C, ...) [mm], zonder boogcorrectie. </summary>
        public IReadOnlyList<double> BeenLengtes()
        {
            var lengtes = new double[Math.Max(0, Punten.Count - 1)];
            for (var i = 0; i < lengtes.Length; i++)
            {
                lengtes[i] = Punt3D.Afstand(Punten[i], Punten[i + 1]);
            }

            return lengtes;
        }

        /// <summary> Buighoek per knik [°] (0° = recht doorgaand). </summary>
        public IReadOnlyList<double> Hoeken()
        {
            var hoeken = new double[AantalKnikken];
            for (var i = 0; i < hoeken.Length; i++)
            {
                var v1 = (Punten[i + 1] - Punten[i]).Genormaliseerd();
                var v2 = (Punten[i + 2] - Punten[i + 1]).Genormaliseerd();
                var cos = Math.Clamp(Punt3D.Dot(v1, v2), -1, 1);
                hoeken[i] = Math.Acos(cos) * 180 / Math.PI;
            }

            return hoeken;
        }

        /// <summary>
        /// Uitgeslagen (getrokken) lengte van de staaf [mm]: som van de beenlengtes
        /// met boogcorrectie per knik (booglengte over de hartlijn i.p.v. de
        /// snijpuntlengte van de benen).
        /// </summary>
        public double UitgeslagenLengte(double diameter)
        {
            var lengte = BeenLengtes().Sum();
            var hoeken = Hoeken();
            for (var i = 0; i < hoeken.Count; i++)
            {
                var hoekRad = hoeken[i] * Math.PI / 180;
                if (hoekRad <= 0)
                {
                    continue;
                }

                var r = BuigstraalBijKnik(i);
                // correctie: 2·r·tan(α/2) (snijpunt) → r·α (boog)
                lengte -= 2 * r * Math.Tan(hoekRad / 2) - r * hoekRad;
            }

            return lengte;
        }

        /// <summary>
        /// Liggen alle punten in één vlak? Zo ja, geeft <paramref name="normaal"/>
        /// de vlaknormaal (voor 2D-buigstaatweergave).
        /// </summary>
        public bool IsVlak(out Punt3D normaal, double tolerantie = 1e-6)
        {
            normaal = default;
            if (Punten.Count < 3)
            {
                normaal = new Punt3D(0, 0, 1);
                return true;
            }

            // eerste niet-gedegenereerde normaal zoeken
            var p0 = Punten[0];
            var v1 = (Punten[1] - p0).Genormaliseerd();
            foreach (var p in Punten.Skip(2))
            {
                var n = Punt3D.Cross(v1, p - p0);
                if (n.Lengte > tolerantie)
                {
                    normaal = n.Genormaliseerd();
                    break;
                }
            }

            if (normaal.Lengte == 0)
            {
                // alle punten collineair → recht: kies willekeurig vlak
                normaal = new Punt3D(0, 0, 1);
                return true;
            }

            var referentie = normaal;
            return Punten.All(p => Math.Abs(Punt3D.Dot(referentie, p - p0)) <= tolerantie * Math.Max(1, (p - p0).Lengte));
        }

        /// <summary> Puntsgewijze lineaire interpolatie tussen twee shapes (zelfde aantal punten). </summary>
        public static StaafShape Lerp(StaafShape a, StaafShape b, double t)
        {
            if (a.Punten.Count != b.Punten.Count)
            {
                throw new ArgumentException("Shapes moeten hetzelfde aantal punten hebben om te interpoleren.");
            }

            return new StaafShape
            {
                Punten = [.. a.Punten.Select((p, i) => Punt3D.Lerp(p, b.Punten[i], t))],
                Buigstralen = [.. a.Buigstralen],
            };
        }

        /// <summary> Kopie van deze shape, getransleerd over <paramref name="offset"/>. </summary>
        public StaafShape Transleer(Punt3D offset) => new()
        {
            Punten = [.. Punten.Select(p => p + offset)],
            Buigstralen = [.. Buigstralen],
        };

        /// <summary>
        /// Valideert de shape: minimaal 2 punten en buigstralen met lengte 0, 1 of n−2.
        /// </summary>
        public void Valideer()
        {
            if (Punten.Count < 2)
            {
                throw new InvalidOperationException("Een staafvorm heeft minimaal 2 punten nodig.");
            }

            if (Buigstralen.Length > 1 && Buigstralen.Length != AantalKnikken)
            {
                throw new InvalidOperationException(
                    $"Buigstralen: lengte 1 (voor alle knikken) of {AantalKnikken} (per knik) verwacht, maar was {Buigstralen.Length}.");
            }

            if (AantalKnikken > 0 && Buigstralen.Length == 0)
            {
                throw new InvalidOperationException("Een gebogen staafvorm heeft minimaal één buigstraal nodig.");
            }
        }
    }
}
