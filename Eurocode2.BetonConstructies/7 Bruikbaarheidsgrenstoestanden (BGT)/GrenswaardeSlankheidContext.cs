using CommonLibrary;
using Profielen.Parametrisch;
using K = CommonLibrary.EurocodeKeys;

namespace Eurocode.BetonConstructies
{
    public class GrenswaardeSlankheidContext : BaseEurocodeContext
    {
        public override string Heading { get; set; } = "Slankheid";
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

        // oorzaken
        private double _lengteOverspanning = 2000;
        private BendingResults? _bendingResults;

        [TableColumn(Label = "Lengte", Symbol = "<i>l</i>", Unit = "mm", Key = K.Slankheid_LengteOverspanning)]
        public double LengteOverspanning
        {
            get => _lengteOverspanning;
            set
            {
                if (SetProperty(ref _lengteOverspanning, value))
                {
                    BerekenEnValideer();
                }
            }
        } // Lengte van de overspanning van het element in mm



        [TableColumn(Label = "effectieve dikte", Symbol = "<i>d</i>", Unit = "mm", Key = K.Slankheid_EffectieveDikte)]
        public double EffectieveDikte
        {
            get => BendingResults?.D ?? 100;


        } // Effectieve dikte van het element in mm


        public ParametrischProfielContext Profiel { get; set; } = new();

        public required BendingResults BendingResults
        {
            get => _bendingResults;
            set
            {
                if (SetNestedProperty(ref _bendingResults, value))
                {
                    BerekenEnValideer();
                }
            }


        }


        // gevolgen
        [TableColumn(Label = "grenswaarde slankheid", Symbol = "(<i>l/d</i><sub>max</sub>", Key = K.Slankheid_Grenswaarde)]
        public double GrenswaardeSlankheid
        {
            get
            {
                var grenswaarde = GrenswaardeSlankheidService.GetGrenswaardeSlankheid(this);
                return grenswaarde;
            }
        } // Grenswaarde van de slankheid van het element

        [TableColumn(Label = "slankheid", Symbol = "(<i>l/d</i>)", Key = K.Slankheid_Slankheid)]
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


        [TableColumn("factor K", Key = K.Slankheid_FactorK)]
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

        /// <summary>
        /// is de referentiewaarde van de wapeningsverhouding = 10-3 · √fck;
        /// </summary>
        [TableColumn(Label = "wapeningsverhouding (referentiewaarde)", Symbol = "<i>ρ</i><sub>0</sub>", Key = K.Slankheid_WapeningsVerhoudingReferentiewaarde, StringFormat = "0.####")]
        public double Rho0
        {
            get
            {
                return 0.001 * Math.Sqrt(BendingResults.Beton.Fck);
            }
        } // is de referentiewaarde van de wapeningsverhouding = 10-3 · √fck;

        [TableColumn(Label = "wapeningsverhouding (vereist)", Symbol = "<i>ρ</i>", Key = K.Slankheid_WapeningsVerhoudingTrekVereist, StringFormat = "0.####")]
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

        public double RhoDrukwapening { get; set; }

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

        private ConstructiefSysteemEnum? _constructiefSysteem = ConstructiefSysteemEnum.VrijOpgelegd;

        [TableColumn(Label = "constructief systeem")]
        public ConstructiefSysteemEnum? ConstructiefSysteem
        {
            get => _constructiefSysteem;
            set
            {
                if (SetProperty(ref _constructiefSysteem, value))
                {
                    Valideer();
                }
            }
        }






        protected override void Bereken()
        {
            // Bereken de grenswaarde van de slankheid
            // automatisch aangeroepen bij het aanroepen van de property GrenswaardeSlankheid

        }

        protected override bool Valideer()
        {
            Meldingen.Clear();

            bool returnVal = true;
            if (GrenswaardeSlankheid > 0 && Slankheid > 0)
            {
                if (Slankheid > GrenswaardeSlankheid)
                {
                    AddMeldingWaarschuwing($"De slankheid (l/d) van het element ({Slankheid:0.#}) is groter dan de grenswaarde ({GrenswaardeSlankheid:0.#}). Toetsing doorbuiging noodzakelijk.");
                }

                if (Slankheid < GrenswaardeSlankheid)
                {
                    AddMeldingOpmerking("Toetsing doorbuiging kan achterwege blijven.");
                }
            }



            return returnVal;
        }

        public override string ToString()
        {
            if (Slankheid <= GrenswaardeSlankheid)
            {
                return $"l/d = {Slankheid:0.#} ≤ {GrenswaardeSlankheid: 0.#} (doorbuiging berekening kan achterwege blijven conform 7.4.2.)";
            }
            else return $"l/d = {Slankheid:0.#} > {GrenswaardeSlankheid: 0.#} (doorbuiging controle berekening nodig)";


        }

        //public override MarkupString ToHtml(bool isDraaiTabel = true)
        //{
        //    return this.ToHtmlTable(isDraaiTabel);
        //    //var dt = this.ToDataTable();
        //    //var type = typeof(GrenswaardeSlankheidContext);
        //    //var mdd = dt.ToMigraDocDocument(type);
        //    //var html = ExportFactory.Services.HtmlCreator.GenerateHtmlFromDocument(mdd);

        //    //return new MarkupString(html);
        //}
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
            var fck = context.BendingResults.Beton.Fck;

            if (context.Rho <= context.Rho0)
            {
                // (7.16.a)
                context.Artikel = "7.16a";
                returnVal = context.FactorK *
                    (11 + 1.5 * Math.Sqrt(fck) * (context.Rho0 / context.Rho) + 3.2 * Math.Sqrt(fck) * Math.Pow((context.Rho0 / context.Rho - 1), 1.5));

                context.Formule = $"{context.FactorK:0.#}×[11+1.5×√({fck:0})×{context.Rho0 / context.Rho:0.###}+3.2×√({fck:0})×{(context.Rho0 / context.Rho - 1):0.###}^1.5]";

            }
            else
            {
                // 7.16.b
                context.Artikel = "7.16b";
                returnVal = context.FactorK *
                    (11 + 1.5 * Math.Sqrt(fck) * (context.Rho0 / (context.Rho - context.RhoDrukwapening)) + 1.0 / 12.0 * Math.Sqrt(fck) * Math.Sqrt((context.RhoDrukwapening / context.Rho0)));

                context.Formule = $"{context.FactorK:0.#}×[11+1.5×√({fck:0})×{context.Rho0 / (context.Rho - context.RhoDrukwapening):0.###}+1/12×√({fck:0})×√({(context.RhoDrukwapening / context.Rho):0.###})]";
            }


            return returnVal;
        }




        public static double GetRho(double dsnOppBeton, double dsnOppWapeningBenodigd)
        {
            return dsnOppWapeningBenodigd / dsnOppBeton;
        }

    }

}
