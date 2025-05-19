using CommonLibrary;

namespace Eurocode.BetonConstructies
{
    public class GrenswaardeSlankheidContext : BaseEurocodeContext
    {
        //7.4.2 Gevallen waarin berekeningen achterwege mogen blijven 

        //(1)P In het algemeen is het niet nodig doorbuigingen expliciet te berekenen, omdat eenvoudige regels kunnen
        //zijn geformuleerd, bijvoorbeeld grenzen voor de slankheid, die toereikend zijn om onder normale
        //omstandigheden doorbuigingsproblemen te vermijden.Zorgvuldigere controles zijn nodig voor elementen die
        //buiten deze grenzen liggen, of als andere doorbuigings-grenzen, dan die welke zijn verwerkt in de
        //vereenvoudigde methoden, van toepassing zijn.

        //(2) Indien gewapend betonnen balken of platen in gebouwen zo zijn gedimensioneerd dat ze voldoen aan de 
        //in deze paragraaf gegeven grenzen van de slankheid, mag ervan zijn uitgegaan dat hun doorbuigingen de in 
        //7.4.1 (4) en(5) gestelde grenzen niet overschrijden.De grenswaarde van de slankheid mag zijn geschat met
        //de vergelijkingen (7.16.a) en(7.16.b) en vermenigvuldigd met correctiefactoren voor het gebruikte type
        //wapening en andere variabelen.Bij de afleiding van deze vergelijkingen is geen rekening gehouden met een
        //eventuele zeeg.


        // oorzaak
        public double LengteOverspanning { get; set; } // Lengte van de overspanning van het element in mm
        public double EffectieveDikte
        {
            get
            {
                if (BendingResults == null) return 100;

                var nuttigeHoogte = BendingResults.D;
                return nuttigeHoogte;
            }

        } // Effectieve dikte van het element in mm




        public BetonContext Beton { get; set; } = new BetonContext();

        public ParametrischeProfielen.ParametrischProfielContext Profiel { get; set; } = new();

        public required BendingResults BendingResults { get; set; }


        // gevolg
        public double GrenswaardeSlankheid
        {
            get
            {
                var grenswaarde = GrenswaardeSlankheidService.GetGrenswaardeSlankheid(this);
                return grenswaarde;
            }
        } // Grenswaarde van de slankheid van het element

        public double Slankheid
        {
            get
            {
                if (EffectieveDikte > 1)
                {
                    return LengteOverspanning / EffectieveDikte; // Slankheid van het element 
                }
                else return 9999;
            }
        } // Slankheid van het element (l/d)



        public double FactorK
        {
            get
            {
                return ConstructiefSysteem switch
                {
                    ConstructiefSysteemEnum.VrijOpgelegd => 1.0,
                    ConstructiefSysteemEnum.EindVeld => 1.3,
                    ConstructiefSysteemEnum.TussenVeld => 1.5,
                    ConstructiefSysteemEnum.Plaatvloer => 1.2,
                    ConstructiefSysteemEnum.Uitkraging => 0.4,
                    _ => throw new ArgumentOutOfRangeException(),
                };
            }
        } // K is een factor om de verschillende constructieve systemen in rekening te brengen

        public double Rho0
        {
            get
            {
                return 0.001 * Math.Sqrt(Beton.Fck);
            }
        } // is de referentiewaarde van de wapeningsverhouding = 10-3 · √fck;

        public double Rho
        {
            get
            {
                if (BendingResults != null)
                {
                    double dsnOppBeton = BendingResults.Profiel?.Area ?? Profiel?.Area ?? 0;
                    return GrenswaardeSlankheidService.GetRho(dsnOppBeton, BendingResults.AsRequired);
                }
                else
                {
                    return 0;
                }
            }
        } // is de vereiste wapeningsverhouding van de trekwapening in het midden van de overspanning
          // (bij uitkragingen ter plaatse van de oplegging)
          // waarmee het moment ten gevolge van de rekenwaarde van de belastingen kan zijn opgenomen

        public double RhoDrukwapening { get; set; } // is de vereiste wapeningsverhouding van de trekwapening in het midden van de overspanning

        public string Artikel { get; set; } = ""; // Artikelnummer van de formule die is gebruikt om de grenswaarde van de slankheid te berekenen
        public string Formule { get; set; } = ""; // Formule die is gebruikt om de grenswaarde van de slankheid te berekenen



        public enum ConstructiefSysteemEnum
        {
            VrijOpgelegd,
            EindVeld,
            TussenVeld,
            Plaatvloer,
            Uitkraging
        }

        public ConstructiefSysteemEnum? ConstructiefSysteem { get; set; } = ConstructiefSysteemEnum.VrijOpgelegd; // Constructief systeem van het element

        public override bool IsAkkoord()
        {
            if (GrenswaardeSlankheid > 0 && Slankheid > 0)
            {
                return Slankheid <= GrenswaardeSlankheid;
            }
            else
            {
                return false;
            }
            throw new NotImplementedException();
        }

        protected override void Bereken()
        {
            // Bereken de grenswaarde van de slankheid
            // automatisch aangeroepen bij het aanroepen van de property GrenswaardeSlankheid

        }

        protected override bool Valideer()
        {
            return IsAkkoord();
        }

        public override string ToString()
        {
            if (Slankheid <= GrenswaardeSlankheid)
            {
                return $"l/d = {Slankheid:0.#} ≤ {GrenswaardeSlankheid: 0.#} (doorbuiging berekening kan achterwege blijven conform 7.4.2.)";
            }
            else return $"l/d = {Slankheid:0.#} > {GrenswaardeSlankheid: 0.#} (doorbuiging controle berekening nodig)";


        }

    }






    public class GrenswaardeSlankheidService
    {

        public static double GetRho0(double fck)
        {
            return 0.001 * Math.Sqrt(fck);
        }


        public static double GetGrenswaardeSlankheid(GrenswaardeSlankheidContext context)
        {
            if (context.Rho == 0)
                return 0;

            double returnVal;
            if (context.Rho <= context.Rho0)
            {
                // (7.16.a)
                context.Artikel = "7.16a";
                returnVal = context.FactorK *
                    (11 + 1.5 * Math.Sqrt(context.Beton.Fck) * (context.Rho0 / context.Rho) + 3.2 * Math.Sqrt(context.Beton.Fck) * Math.Pow((context.Rho0 / context.Rho - 1), 1.5));

                context.Formule = $"{context.FactorK:0.#}×[11+1.5×√({context.Beton.Fck:0})×{context.Rho0 / context.Rho:0.###}+3.2×√({context.Beton.Fck:0})×{(context.Rho0 / context.Rho - 1):0.###}^1.5]";

            }
            else
            {
                // 7.16.b
                context.Artikel = "7.16b";
                returnVal = context.FactorK *
                    (11 + 1.5 * Math.Sqrt(context.Beton.Fck) * (context.Rho0 / (context.Rho - context.RhoDrukwapening)) + 1.0 / 12.0 * Math.Sqrt(context.Beton.Fck) * Math.Sqrt((context.RhoDrukwapening / context.Rho0)));

                context.Formule = $"{context.FactorK:0.#}×[11+1.5×√({context.Beton.Fck:0})×{context.Rho0 / (context.Rho - context.RhoDrukwapening):0.###}+1/12×√({context.Beton.Fck:0})×√({(context.RhoDrukwapening / context.Rho):0.###})]";
            }


            return returnVal;
        }




        public static double GetRho(double dsnOppBeton, double dsnOppWapeningBenodigd)
        {
            return dsnOppWapeningBenodigd / dsnOppBeton;
        }

    }

}
