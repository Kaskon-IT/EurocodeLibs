using Eurocode.BetonConstructies;


namespace Eurocode2.BetonConstructies;

/// <summary>Toetsresultaat voor één staafgroep (langswapening of beugels).</summary>
public class J3ConsoleStaafgroepToets
{
    public required string Naam { get; init; }
    public required WapeningGroep Groep { get; init; }
    public bool IsLangswapening { get; init; }

    /// <summary>Aantal staafdoorsneden in de beschouwde snede/zone.</summary>
    public int AantalDoorsneden { get; init; }
    public double AsAanwezig { get; init; }      // mm²
    public double AsBenodigd { get; init; }      // mm²
    public double SigmaS { get; init; }          // N/mm²
    public double Fyd { get; init; }             // N/mm²

    public double UcDoorsneden => AsAanwezig > 0 ? AsBenodigd / AsAanwezig : double.PositiveInfinity;

    // ---- Alleen langswapening ----
    public List<VerankeringResult> Verankeringen = [ ];
    //public VerankeringResult VerankeringStart { get; set; } = new();
    public double? VerankeringBenodigd { get; init; }   // lbd [mm]
    public double? VerankeringAanwezig { get; init; }   // [mm]
    public double? BuigdoornToegepast { get; init; }    // [mm]
    public double? BuigdoornBenodigd { get; init; }     // [mm]

    public bool VoldoetDoorsneden => UcDoorsneden <= 1.0;
    public bool VoldoetVerankering => VerankeringBenodigd is null || VerankeringAanwezig is null || VerankeringAanwezig >= VerankeringBenodigd;
    public bool VoldoetBuigdoorn => BuigdoornBenodigd is null || BuigdoornToegepast is null || BuigdoornToegepast >= BuigdoornBenodigd;
    public bool Voldoet => VoldoetDoorsneden && VoldoetVerankering && VoldoetBuigdoorn;
}

/// <summary>Trekband bovenin: AsMain + wringing versus doorsneden haarspelden op snede x=0.</summary>
public class J3ConsoleTrekbandToets
{
    public double AsMainReq { get; init; }
    public double AslTorsie { get; init; }
    public double AsBenodigd => AsMainReq + AslTorsie;
    public int DoorsnedenVerticaal { get; init; }
    public int DoorsnedenHorizontaal { get; init; }
    public double AsAanwezig { get; init; }
    public double Uc => AsAanwezig > 0 ? AsBenodigd / AsAanwezig : double.PositiveInfinity;
    public bool Voldoet => Uc <= 1.0;
}


public static class VerankeringResultExtensions
{
    public static void AddOrUpdate(
        this List<VerankeringResult> verankeringen,
        VerankeringResult verankering)
    {
        var index = verankeringen.FindIndex(x => x.Id == verankering.Id);

        if (index >= 0)
            verankeringen[index] = verankering;
        else
            verankeringen.Add(verankering);
    }
}

public static class J3ConsoleStaafgroepToetsen
{
    /// <summary>Aan te roepen ná J3ConsoleWapeningBuilder.VulGroepen(i, result).</summary>
    public static void Toets(J3ConsoleInput i, J3ConsoleResult r)
    {
        r.StaafgroepToetsen.Clear();

        // ---- Trekband bovenin (snede x = 0) ----
        int nVer = TelDoorsnedenX0(r.WapVerticaleHaarspelden, 0);
        int nHor = TelDoorsnedenX0(r.WapHorizontaleHaarspelden, 0);
        double asVer = WapeningHelper.GetDsnOpp(nVer, r.WapVerticaleHaarspelden?.Diameter ?? 0);
        Console.WriteLine($"[.Toets] As,haarspeld = {asVer}");
        double asHor = WapeningHelper.GetDsnOpp(nHor, r.WapHorizontaleHaarspelden?.Diameter ?? 0);
        Console.WriteLine($"[.Toets] As,haarspeld,plat = {asHor}");


        double asTotaal = asVer + asHor;


        double aslTorsie = /* aandeel extra langswapening uit wringing bovenin, zie noot */ 0.0;

        r.TrekbandToets = new J3ConsoleTrekbandToets
        {
            AsMainReq = r.AsMain,
            AslTorsie = aslTorsie,
            DoorsnedenVerticaal = nVer,
            DoorsnedenHorizontaal = nHor,
            AsAanwezig = asTotaal,
        };

        // Trekband-eis proportioneel verdelen naar aanwezige doorsnede per groep
        double asReq = r.TrekbandToets.AsBenodigd;
        double fVer = asTotaal > 0 ? asVer / asTotaal : 0;
        double fHor = asTotaal > 0 ? asHor / asTotaal : 0;

       


        if (r.WapVerticaleHaarspelden is { } gv)
        {
            // bepaal even de lengte na de verankering.
            var p0 = gv.Shape.Punten[0];
            var p1 = gv.Shape.Punten[1];
            List<Punt3D> punten = new() {
                p0,
                p1,
                new(-r.X1/2.0, p1.Y, p1.Z)
                };
            StaafShape vorm = new() { Buigstralen = gv.Shape.Buigstralen, Punten = punten };

            var lProv = vorm.UitgeslagenLengte(gv.Diameter);

        
            r.StaafgroepToetsen.Add(MaakLangsToets("Trekband wapening", gv, nVer, asVer,
                asReq * fVer, r.MainFy, r.Fcd, (int)i.Fck, lProv));
        }
            

        if (r.WapHorizontaleHaarspelden is { } gh)
            r.StaafgroepToetsen.Add(MaakLangsToets("Trekband (platte hs)", gh, nHor, asHor,
                asReq * fHor, r.MainFy, r.Fcd, (int)i.Fck, gh.UitgeslagenLengte));
        // ---- Horizontale beugels: zone y = d1 … d1 + 2/3·d ----
        if (r.WapBglsHor is { } gbh)
        {
            double yMin = r.D1, yMax = r.D1 + 2.0 / 3.0 * r.D;
            int n = TelStavenInZoneY(gbh, yMin, yMax);
            double asAanw = WapeningHelper.GetDsnOpp(n, gbh.Diameter) * 2; // 2 benen per beugel
            double sigma = asAanw > 0 ? r.Asw / asAanw * r.MainFy : double.PositiveInfinity;

            r.StaafgroepToetsen.Add(new J3ConsoleStaafgroepToets
            {
                Naam = "Horizontale beugels (in zone ⅔·d)",
                Groep = gbh,
                IsLangswapening = false,
                AantalDoorsneden = n,
                AsAanwezig = asAanw,
                AsBenodigd = r.Asw,
                SigmaS = sigma,
                Fyd = r.MainFy,
            });
        }

        // ---- Verticale beugels ----
        if (r.WapBglsVer is { } gbv)
        {
            int n = gbv.AantalPosities;
            // todo: tel in zone av.
            double asAanw = WapeningHelper.GetDsnOpp(n, gbv.Diameter) * 2;
            double asReqBgl = 0.0; // benodigde Asw uit wringing indien van toepassing
            double sigma = asAanw > 0 ? asReqBgl / asAanw * r.MainFy : 0.0;

            r.StaafgroepToetsen.Add(new J3ConsoleStaafgroepToets
            {
                Naam = "Verticale beugels (in zone av)",
                Groep = gbv,
                IsLangswapening = false,
                AantalDoorsneden = n,
                AsAanwezig = asAanw,
                AsBenodigd = asReqBgl,
                SigmaS = sigma,
                Fyd = r.MainFy,
            });
        }

        // Door de builder berekende verankeringen (met werkelijke staafvorm)
        // overnemen als VerankeringStart, waar beschikbaar.
        foreach (var toets in r.StaafgroepToetsen)
        {
            if (r.VerankeringenPerGroep.TryGetValue(toets.Groep, out var verankeringen))
                toets.Verankeringen = [.. verankeringen];
        }
    }

    private static J3ConsoleStaafgroepToets MaakLangsToets(
        string naam, WapeningGroep g, int n, double asAanwezig,
        double asBenodigd, double fyd, double fcd, int fck, double verankeringAanwezig)
    {
        double benutting = asAanwezig > 0 ? Math.Min(1.0, asBenodigd / asAanwezig) : 1.0;
        double sigma = benutting * fyd;
        double staafDsn = WapeningHelper.GetDsnOpp(1, g.Diameter);

        double ab = 30; // todo: wijzig naar c + phi/2.0; 
        double s = g.Tussenruimte() + g.Diameter;
        if (s > 0)
            ab = Math.Min(s/2, ab); // kleinste ab een s/2;

        double fbt = staafDsn * sigma ; // trekkracht per staaf [N]



        //double buigdoornMin = Math.Max(
        //  WapeningHelper.GetBuigdoorMin(g.Diameter),
        //           WapeningHelper.GetBuigdoornMin(fcd, ab, fbt, g.Diameter));

        //var verankering = WapeningHelper.GetVerankeringResult(ab, fbt, g.EquivalenteDiameter(), fck, benutting);
        //verankering.StaafType = VerankeringStaafType.Trekstaaf;
        //verankering.StaafVorm = VerankeringStaafVorm.Gebogen;


        



        double buigdoornToegepast = 0.0;
        if (g.Buigstralen != null) buigdoornToegepast = g.Buigstralen.Max() * 2.0;
        else buigdoornToegepast = 0.0;


        return new J3ConsoleStaafgroepToets
        {
            Naam = naam,
            Groep = g,
            IsLangswapening = true,
            AantalDoorsneden = n,
            AsAanwezig = asAanwezig,
            AsBenodigd = asBenodigd,
            SigmaS = sigma,
            Fyd = fyd,
            VerankeringAanwezig = verankeringAanwezig,
        };
    }

    /// <summary>Telt staafdoorsneden die het vlak x=0 kruisen (benen van de haarspelden).</summary>
    public static int TelDoorsnedenX0(WapeningGroep? g, double x, double y1 = 0, double y2 = 100, double z1 = -100, double z2 = 200)
            {
        if (g is null) return 0;
        int totaal = 0;
        var staven = g.GenereerStaven();

        

        Console.WriteLine($"[TelDoorsnedenX0] {g.TotaalAantalStaven}Ø{g.Diameter}");
        Console.WriteLine("[TelDoorsnedenX0] Aantal voor tellen: groep.Aantal={0} : staven.Aantal= {1} staven", g.TotaalAantalStaven, staven.Count());

        foreach (var staaf in staven)
        {
            var p = staaf.Shape.Punten;
            for (int i = 0; i < p.Count - 1; i++)
                if ((p[i].X <= x && p[i + 1].X > x) || (p[i].X > x && p[i + 1].X <= x))
                {
                    // todo: vind de doorsnede van het lijnstuk met het vlak x=0 en controleer of y1 <= y <= y2 en z1 <= z <= z2

                    // voorlopig controleer of beide punten binnen de y- en z-bereiken liggen
                    if ((p[i].Y < y1 && p[i + 1].Y < y1) || (p[i].Y > y2 && p[i + 1].Y > y2)) continue;
                    if ((p[i].Z < z1 && p[i + 1].Z < z1) || (p[i].Z > z2 && p[i + 1].Z > z2)) continue;

                    Console.WriteLine($"[TelDoorsnedenX0] totaal++, want P{i}: ({p[i].X}, {p[i].Y}) -> P{i+1}: ({p[i + 1].X}, {p[i + 1].Y})");
                    totaal++;

                }
        }
        Console.WriteLine("[TelDoorsnedenX0] Aantal na tellen: totaal={0}", totaal);


        return totaal;
    }

    /// <summary>Telt beugeldoorsneden waarvan de staaf binnen yMin..yMax ligt.</summary>
    public static int TelStavenInZoneY(WapeningGroep g, double yMin, double yMax)
    {
        int totaal = 0;
        foreach (var staaf in g.GenereerStaven())
        {
            double y = staaf.Shape.Punten.Average(p => p.Y);
            if (y >= yMin && y <= yMax) totaal += 1;
        }
        return totaal;
    }
}