namespace Eurocode.BetonConstructies.Tests
{
    using Eurocode.BetonConstructies;

    public class WapeningGroepTests
    {
        private static StaafShape RechteStaaf(double lengte = 1000) => new()
        {
            Punten = [new Punt3D(0, 0, 0), new Punt3D(lengte, 0, 0)],
        };

        private static WapeningGroep Groep(WapeningVerdeling verdeling, double verdeelLengte = 900) => new()
        {
            Diameter = 12,
            Shapes = [RechteStaaf()],
            VerdeelStart = new Punt3D(0, 0, 0),
            VerdeelEind = new Punt3D(0, 0, verdeelLengte),
            Verdeling = verdeling,
        };

        [Fact]
        public void Gelijkmatig_VerdeeltStavenOverDeLijn()
        {
            var groep = Groep(new WapeningVerdeling { Type = VerdelingType.Gelijkmatig, Aantal = 4 });

            var staven = groep.GenereerStaven().ToList();

            Assert.Equal(4, staven.Count);
            Assert.Equal(300, groep.WerkelijkeHartOpHart, 6);
            Assert.Equal([0, 300, 600, 900], staven.Select(s => s.Shape.Punten[0].Z));
        }

        [Fact]
        public void ExacteHartOpHart_AantalEnEindpuntVolgenUitAfstanden()
        {
            var groep = Groep(new WapeningVerdeling
            {
                Type = VerdelingType.ExacteHartOpHart,
                HartOpHartAfstanden = [50, 150, 150, 150, 50],
            });

            var staven = groep.GenereerStaven().ToList();

            Assert.Equal(6, groep.AantalPosities);          // n afstanden → n + 1 staven
            Assert.Equal(550, groep.VerdeelLengte, 6);       // Σsᵢ
            Assert.Equal([0, 50, 200, 350, 500, 550], staven.Select(s => s.Shape.Punten[0].Z));
        }

        [Fact]
        public void BeoogdeHartOpHart_BerekentAantalEnVerdeeltGelijkmatig()
        {
            var groep = Groep(new WapeningVerdeling
            {
                Type = VerdelingType.BeoogdeHartOpHart,
                BeoogdeHartOpHart = 250,
            });

            // L = 900, s = 250 → ceil(900/250) + 1 = 5 posities, h.o.h. = 225 ≤ 250
            Assert.Equal(5, groep.AantalPosities);
            Assert.Equal(225, groep.WerkelijkeHartOpHart, 6);
            Assert.True(groep.WerkelijkeHartOpHart <= 250);
        }

        [Fact]
        public void TweeShapes_InterpoleertLineair()
        {
            var groep = Groep(new WapeningVerdeling { Type = VerdelingType.Gelijkmatig, Aantal = 3 });
            groep.Shapes = [RechteStaaf(1000), RechteStaaf(2000)];

            var staven = groep.GenereerStaven().ToList();

            Assert.Equal(1000, staven[0].Shape.BeenLengtes()[0], 6);
            Assert.Equal(1500, staven[1].Shape.BeenLengtes()[0], 6);
            Assert.Equal(2000, staven[2].Shape.BeenLengtes()[0], 6);
        }

        [Fact]
        public void EquivalenteDiameter_VolgtParagraaf891()
        {
            var groep = Groep(new WapeningVerdeling { Type = VerdelingType.Gelijkmatig, Aantal = 2 });
            groep.AantalStavenPerPositie = 2;

            Assert.Equal(12 * Math.Sqrt(2), groep.EquivalenteDiameter(), 6);
            Assert.Equal(4, groep.TotaalAantalStaven);
            Assert.Equal(4 * Math.PI / 4 * 144, groep.TotaalAs, 6);
        }

        [Fact]
        public void MinimaleTussenruimte_VolgtParagraaf82()
        {
            var groep = Groep(new WapeningVerdeling { Type = VerdelingType.Gelijkmatig, Aantal = 2 });

            Assert.Equal(37, groep.MinimaleTussenruimte(32));  // dg + 5 maatgevend
            Assert.Equal(20, groep.MinimaleTussenruimte(8));   // 20 mm maatgevend
        }

        [Fact]
        public void Valideer_GooitBijOngeldigeInvoer()
        {
            var groep = Groep(new WapeningVerdeling { Type = VerdelingType.Gelijkmatig, Aantal = 0 });
            Assert.Throws<InvalidOperationException>(groep.Valideer);

            var groep2 = Groep(new WapeningVerdeling { Type = VerdelingType.ExacteHartOpHart });
            Assert.Throws<InvalidOperationException>(groep2.Valideer);
        }
    }

    public class StaafShapeTests
    {
        private static StaafShape LVorm() => new()
        {
            // L-vorm: been 500 + been 300, 90° knik, r = 24
            Punten = [new Punt3D(0, 0, 0), new Punt3D(500, 0, 0), new Punt3D(500, 300, 0)],
            Buigstralen = [24],
        };

        [Fact]
        public void BeenLengtesEnHoeken()
        {
            var shape = LVorm();

            Assert.Equal([500, 300], shape.BeenLengtes());
            Assert.Equal(90, shape.Hoeken()[0], 6);
        }

        [Fact]
        public void UitgeslagenLengte_MetBoogcorrectie()
        {
            var shape = LVorm();

            // 800 − (2·24·tan45° − 24·π/2) = 800 − (48 − 37.699) = 789.699
            Assert.Equal(800 - (48 - 24 * Math.PI / 2), shape.UitgeslagenLengte(12), 3);
        }

        [Fact]
        public void IsVlak_DetecteertVlakEnNietVlak()
        {
            Assert.True(LVorm().IsVlak(out var normaal));
            Assert.Equal(1, Math.Abs(normaal.Z), 6);

            var ruimtelijk = new StaafShape
            {
                Punten = [new Punt3D(0, 0, 0), new Punt3D(100, 0, 0), new Punt3D(100, 100, 0), new Punt3D(100, 100, 100)],
                Buigstralen = [24],
            };
            Assert.False(ruimtelijk.IsVlak(out _));
        }

        [Fact]
        public void Lerp_InterpoleertPuntsgewijs()
        {
            var a = new StaafShape { Punten = [new Punt3D(0, 0, 0), new Punt3D(100, 0, 0)] };
            var b = new StaafShape { Punten = [new Punt3D(0, 0, 0), new Punt3D(200, 0, 0)] };

            var midden = StaafShape.Lerp(a, b, 0.5);

            Assert.Equal(150, midden.Punten[1].X, 6);
        }

        [Fact]
        public void Valideer_ControleertBuigstralen()
        {
            var zonderStraal = new StaafShape
            {
                Punten = [new Punt3D(0, 0, 0), new Punt3D(100, 0, 0), new Punt3D(100, 100, 0)],
            };
            Assert.Throws<InvalidOperationException>(zonderStraal.Valideer);

            var verkeerdAantal = LVorm();
            verkeerdAantal.Buigstralen = [24, 24, 24];
            Assert.Throws<InvalidOperationException>(verkeerdAantal.Valideer);
        }
    }

    public class WapeningStaafTests
    {
        [Fact]
        public void Valideer_VereistPreciesEenShape()
        {
            var staaf = new WapeningStaaf
            {
                Diameter = 16,
                Shapes =
                [
                    new StaafShape { Punten = [new Punt3D(0, 0, 0), new Punt3D(500, 0, 0)] },
                    new StaafShape { Punten = [new Punt3D(0, 0, 0), new Punt3D(500, 0, 0)] },
                ],
            };

            Assert.Throws<InvalidOperationException>(staaf.Valideer);
        }

        [Fact]
        public void AsEnUitgeslagenLengte()
        {
            var staaf = new WapeningStaaf
            {
                Diameter = 16,
                Shapes = [new StaafShape { Punten = [new Punt3D(0, 0, 0), new Punt3D(500, 0, 0)] }],
            };

            staaf.Valideer();
            Assert.Equal(Math.PI / 4 * 256, staaf.As, 6);
            Assert.Equal(500, staaf.UitgeslagenLengte, 6);
        }
    }
}
