using Eurocode.BetonConstructies._6_Uiterste_grenstoestanden__UGT_;

namespace Eurocode.BetonConstructies.Aanvullende_regels_prefab
{
    public class TandMetHals
    {



        public void BerekeningNeus(UitkragingContext context)
        {


            // Hier wordt de neusberekening gedaan:
            // Uitgangspunten:
            // - Lijnvormige ondersteuning!
            // - Gedrongen ligger!	
            // - Geen dwarskrachtwapening nodig!
            // - Relatieve oplegspanning blijft kleiner of gelijk 0,15 (tabel 10.2)!
            // - Toegepaste Momentwapening == Toegepaste Ophangwapening!

            // ------
            // INPUTS		// Deze waarden moeten doorgegeven worden vanuit de TrapBerekening
            // ------

            // L	is lengte console		
            // hc	is hoogte console		
            // Fd	rekenwaarde reactiekracht
            // Hd	rekenwaarde reactiekracht (horizontaal)





            //beton.DoorgaandElement = false;
            //beton.OverspanningOndersteundeElement_l = lengteOndersteundeElement;

            // a2+Δa2
            //beton.Afstand_a2 = 10; // tabel 10.3
            //beton.Afstand_Δa2 = 10 + beton.OverspanningOndersteundeElement_l / 1200; // tabel 10.5
            //if (beton.Afstand_Δa2 > 30) beton.Afstand_Δa2 = 30;

            // a3+Δa3
            //beton.Afstand_a3 = 15; // tabel 10.4
            //beton.Afstand_Δa3 = beton.OverspanningOndersteundeElement_l / 2500; // l / 2500

            // voegen
            //beton.VoegHorizontaal = voegX;
            //beton.VoegVerticaal = voegY;

            // ab	breedte lastvlak
            //beton.BreedteLastvlak_ab = beton.LengteConsole_L - beton.Afstand_Δa3 - beton.Afstand_a3 - beton.Afstand_a2 - beton.Afstand_Δa2 - beton.VoegVerticaal;



            // aanvulling oplegspanning
            //beton.BreedteTussenlaag_b1 =
            //    beton.Afstand_b1 = werkendeBreedte;

            // aanvulling berekende a1 (
            //beton.RekenwaardeDrukSterkteDragendElement_fcd = beton.Fcd;

            // vilt 5,0 kN/m² toepassen (tenzij anders aangegeven)



            //beton.OplegsterkteRekenwaarde_fRd = 0.4 * beton.RekenwaardeDrukSterkteDragendElement_fcd;

            //beton.OplegmateriaalSterkteRekenwaarde_fBed = 5;

            //beton.OplegsterkteRekenwaarde_fRd = Math.Min(beton.OplegsterkteRekenwaarde_fRd, beton.OplegmateriaalSterkteRekenwaarde_fBed);




            //beton.Oplegspanning = beton.Reactie_Fd * 1000 / beton.BreedteLastvlak_ab / beton.BreedteTussenlaag_b1;
            //beton.RelatieveOplegspanning = beton.Oplegspanning / beton.Fcd;


            // lijnvormig, NEN-EN 1992 tabel 10.2
            //if (beton.RelatieveOplegspanning <= 0.15)
            //    beton.Afstand_a1Min = 25;
            //else if (beton.RelatieveOplegspanning < 0.4)
            //    beton.Afstand_a1Min = 30;
            //else
            //    beton.Afstand_a1Min = 40;



            //  a1 = FEd / (b1 fRd)
            //beton.Afstand_a1 = beton.Reactie_Fd * 1000 / (beton.Afstand_b1 * beton.OplegsterkteRekenwaarde_fRd);


            //var nominaleOpleglengte = beton.NominaleOplegLengte;
            //var a = beton.Afstand_a1 + beton.Afstand_a2 + beton.Afstand_a3 + Math.Sqrt(Math.Pow(beton.Afstand_Δa2, 2) + Math.Pow(beton.Afstand_Δa3, 2));

            //beton.NominaleOplegLengte_a = a;

            // 
            var ab = context.Lengte / 2;
            var a_voor_berekening_z = BuigingContext.GetLengteArmInBerekeningUitkraging(ab, context.Lengte, context.Profiel.Hoogte);
            // z
            //double arm = 100;
            //double hoogte = 200;




            // moment
            var dekking = 20.0;
            var staafDiameter = 8;
            var armMoment = context.Lengte / 2 + dekking + staafDiameter / 2;
            context.Arm = armMoment;

            var moment = armMoment * context.Reactiekracht;
            context.Moment = moment;


            var zGedrongen = BuigingContext.GetInwendigeHefboomArmGedrongenLigger(a_voor_berekening_z, context.Profiel.Hoogte, statischBepaald: true, isUitkraging: true);
            var wapAsBen = BuigingContext.GetAsBenodigdGedrongenLigger(moment, zGedrongen, context.Beton);

            // reken ook altijd slank uit. Als dit een lagere weerstand geeft dan is dit maatgevend!
            var buiging = new BuigingBasic() { Beton = context.Beton, D = context.NuttigeHoogte, M = moment };

            // controleer of niet lager
            if (buiging.As > wapAsBen)
            {
                wapAsBen = buiging.As;
            }

            context.AsBen = wapAsBen;



            // av1
            //beton.TussenruimteLastvlak_av1 = beton.LengteConsole_L - beton.Afstand_Δa3 - beton.Afstand_a3 - beton.BreedteLastvlak_ab;

            //// av	art. 6.2.2(6)
            //beton.Afstand_av = beton.TussenruimteLastvlak_av1 + 0.5 * beton.BreedteLastvlak_ab;           // av is tot hart van oplegmateriaal
            //if (beton.Afstand_av <= 0.5 * beton.NuthoogteConsole_d)           // av is minimaal 0,5d
            //{ beton.Afstand_av = 0.5 * beton.NuthoogteConsole_d; }
            //if (beton.Afstand_av >= 2 * beton.NuthoogteConsole_d)             // av is maximaal 2d
            //{ beton.Afstand_av = 2 * beton.NuthoogteConsole_d; }




            //// a
            //beton.AfstandFdTotOplegreactie_a = beton.AfstandOplegreactieTotRand + beton.TussenruimteLastvlak_av1 + 0.5 * beton.BreedteLastvlak_ab;

            //// Lov = 2a
            //beton.TheoretischeOverspanning_Lov = 2 * beton.AfstandFdTotOplegreactie_a;

            //// Lov/hNeusWapeningHOH
            //beton.LovGedeeldDoorHoogte = beton.TheoretischeOverspanning_Lov / beton.HoogteConsole_hc;





            //beton.HulpInwendigeHefboomsarm_z[0] = 0.4 * (beton.AfstandFdTotOplegreactie_a + beton.HoogteConsole_hc);       // 0,4a + 0,4hc
            //beton.HulpInwendigeHefboomsarm_z[1] = 1.6 * beton.AfstandFdTotOplegreactie_a;                                  // 1,6a
            //beton.HulpInwendigeHefboomsarm_z[2] = 0.8 * beton.HoogteConsole_hc;                                            // 0,8h
            //beton.InwendigeHefboomsarmConsole_z = beton.HulpInwendigeHefboomsarm_z.Min();                                  // kleinste


            // aanvulling vanuit Kaskon
            //if (beton.InwendigeHefboomsarmConsole_z > 0.9 * beton.NuthoogteConsole_d)
            //                beton.InwendigeHefboomsarmConsole_z = 0.9 * beton.NuthoogteConsole_d; /// niet groter dan 0,9d

            //          if (beton.InwendigeHefboomsarmConsole_z > beton.HoogteConsole_hc - beton.BetondekkingToegepast - beton.BetondekkingToegepast - beton.WapeningDiameterToegepast)
            //            beton.InwendigeHefboomsarmConsole_z = beton.HoogteConsole_hc - beton.BetondekkingToegepast - beton.BetondekkingToegepast - beton.WapeningDiameterToegepast;


            // Beta		art. 6.2.2(6)
            //beton.NeusDwarskracht_Beta = beton.Afstand_av / (2 * beton.NuthoogteConsole_d);

            //if (beton.NeusDwarskracht_Beta < 0.25)
            //    beton.NeusDwarskracht_Beta = 0.25;



            // Md is moment
            //beton.MomentConsole_Md = beton.AfstandFdTotOplegreactie_a / 1000 * beton.Reactie_Fd + (beton.InwendigeHefboomsarmConsole_z + beton.HoogteConsole_hc - beton.NuthoogteConsole_d) / 1000 * beton.Reactie_Hd;
            //beton.MomentConsole_Md = beton.AfstandFdTotOplegreactie_a / 1000 * beton.Reactie_Fd;
            //beton.MomentConsole_Md += (beton.HoogteConsole_hc + beton.InwendigeHefboomsarmConsole_z - beton.NuthoogteConsole_d) / 1000 * beton.Reactie_Hd;


            // As = Md / fyd / z
            //beton.BenodigdeTrekwapeningNeus_As = beton.MomentConsole_Md * 1000000 / beton.Fyd / beton.InwendigeHefboomsarmConsole_z;

            // Asmin1
            // bepalen scheurmoment ten behoeve van Asmin1
            //double _Weerstandsmoment = beton.BreedteConsole_b * Math.Pow(beton.HoogteConsole_hc, 2) / 6;
            //double _hulpMeMin = beton.Fctm * _Weerstandsmoment / 1000 / 1000;

            // Asmin1 kan nu worden berekend adhv Memin (is constant)
            //double _hulpXu = (beton.NuthoogteConsole_d - (Math.Pow((beton.NuthoogteConsole_d * beton.NuthoogteConsole_d - ((4 * beton.Beta * _hulpMeMin * 1000000) / (beton.Alpha * beton.BreedteConsole_b * beton.Fcd))), 0.5))) / (2 * beton.Beta);
            //double _hulpAsMin1 = (beton.Alpha * beton.BreedteConsole_b * _hulpXu * beton.Fcd) / beton.Fyd;

            //Asmin2
            //Asmin2 is 1,25 x As      
            //double _hulpAsMin2 = 1.25 * beton.BenodigdeTrekwapeningNeus_As;
            //Asmin is kleinste van Asmin1 en Asmin2
            //double _hulpAsMin = Math.Min(_hulpAsMin1, _hulpAsMin2);

            //Asben --> Indien As <= Amin, dan Asmin, anders As
            // zie hieronder de korste manier [? operator]
            //beton.BenodigdeTrekwapeningNeus_As = (_hulpAsMin >= beton.BenodigdeTrekwapeningNeus_As) ? _hulpAsMin : beton.BenodigdeTrekwapeningNeus_As;



            // -------------------
            // Toegepaste wapening BOVEN
            // -------------------
            //beton.NeusWapeningAantalStaven = 1000 / beton.NeusWapeningHOH;


            //beton.NeusWapeningToegepast = TrapExtensions.WapStavenNaarDoorsnedeAs(beton.NeusWapeningAantalStaven, beton.NeusWapeningØ);



            // Controle dwarskracht

            // ----------------------------------------------------------
            // Elementen die geen berekende dwarskrachtwapening vereisen
            // ----------------------------------------------------------
            // factor k voor de neus




            // --------------
            // Ophangwapening
            // --------------

            // NEd_ophang
            //beton.Reactie_Ophang = 2 * beton.Reactie_Fd;
            //beton.BenodigdeOphangwapeningNeus_As = beton.Reactie_Ophang * 1000 / beton.Fyd;



        }


        //public void MaakBerekeningTand(BetonElement betonElement, SteekTrapEntity trap, double reactie_Fd, double werkendeBreedte, bool isTandOnderzijde)
        //{
        //    // verzamel eerst alle context

        //    // kracht.reactiekracht
        //    // kracht.arm


        //    // nok.profiel



        //    // hals.profiel



        //    // uit berekening halen
        //    // + parameters verwijderen/toevoegen
        //    betonElement.LengteConsole_L = trap.AansluitingBoven.TandLengte;
        //    betonElement.HoogteConsole_hc = trap.AansluitingBoven.TandHoogte;
        //    betonElement.Reactie_Fd = reactie_Fd;
        //    // Wat is de wapening in de neus?
        //    Tekla.Structures.Model.RebarGroup? rbg = trap.ToRebarGroupBeugelBoven();


        //    if (isTandOnderzijde)
        //    {
        //        betonElement.LengteConsole_L = trap.AansluitingOnder.TandLengte;
        //        betonElement.HoogteConsole_hc = trap.AansluitingOnder.TandHoogte;
        //        rbg = trap.ToRebarGroupHaarspeldOnder();
        //    }

        //    if (betonElement.HorReactieToepassen == true)
        //    {
        //        betonElement.Reactie_Hd = 0.4 * betonElement.Reactie_Fd;
        //    }
        //    else
        //    {
        //        betonElement.Reactie_Hd = 0;
        //    }


        //    OplegneusLijnvormigGedrongen oplegneus = new OplegneusLijnvormigGedrongen();
        //    double diameter = 6;
        //    double hoh = 150;

        //    if (rbg != null)
        //    {
        //        diameter = double.Parse(rbg.Size);
        //        if (rbg.Spacings != null && rbg.Spacings.Count > 0)
        //        {
        //            hoh = (double)rbg.Spacings[0];
        //        }
        //    }

        //    betonElement.NeusWapeningHOH = hoh;
        //    betonElement.OphangStavenDiameter = diameter;
        //    betonElement.HaarspeldDiameter = diameter;
        //    betonElement.WapeningDiameterToegepast = diameter;
        //    betonElement.NeusWapeningØ = diameter;



        //    double voegX = 10;
        //    double voegY = 5;

        //    // bereken de oplegneus met de verzameling gegevens tussen de haakjes
        //    oplegneus.BerekeningNeus(betonElement, trap.TotaleLengte, werkendeBreedte, voegX, voegY);


        //}


    }




}
