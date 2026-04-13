using CommonLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eurocode.StaalConstructies
{



    public class StaalContext : BaseMateriaal
    {
        public override MateriaalType Type => MateriaalType.Staal;
        public override double SoortelijkGewicht => 7850; // kg/m³
        public override double E => 210e3;
        public override string Naam => StaalKwaliteit?.ToString() ?? "?";
        

        // Partiele materiaalfactoren (6.1
        //        6.1 Algemeen
        //(1) De in 2.4.3 gedefinieerde partiële factoren M behoren te zijn toegepast op de in dit hoofdstuk vermelde
        //verschillende karakteristieke waarden van de weerstand van de doorsnede, en wel als volgt: 
        //— weerstand van de doorsneden, voor elke klasse: GammaM0
        //— weerstand van elementen op gebied van stabiliteit via een toetsing van de elementen: GammaM1
        //— weerstand van de doorsneden in trek tot aan de breuk: GammaM2
        public override double GammaM0 { get => base.GammaM0; set => base.GammaM0 = value; }
        public override double GammaM1 { get => base.GammaM1; set => base.GammaM1 = value; }
        public override double GammaM2 { get => base.GammaM2; set => base.GammaM2 = value; }

        public StaalContext()
        {
            PartieleFactor = 1.0; 
        }




        public override string UserFriendlyName => StaalKwaliteit?.ToString() ?? "Onbekend";


        public StaalKwaliteitEnum? StaalKwaliteit { get; set; } = StaalKwaliteitEnum.S235;

        private double _poisson = 0.3;

        public double Fy => StaalKwaliteit.GetFy();
        public double Fu => StaalKwaliteit.GetFu();
        public double Poisson
        {
            get => _poisson;
            set => _poisson = value;
        }



        /// <summary>
        /// Elasticiteitsmodulus
        /// </summary>
        //public static double E => 210000;
        /// <summary>
        /// Glijdingsmodulus
        /// </summary>
        public double G => E / (2 * (1+ _poisson));

        /// <summary>
        /// De lineaire-thermische-uitzettingscoefficient
        /// alpha = 12 × 10–6 per K (voor T <= 100 °C) 
        /// </summary>
        public static double LineaireThermischeUitzettingsFactor => 12e-6;



        public override string Eurocode => "EC3 - Staalconstructies";

        protected override void Bereken()
        {
            //
        }

        protected override bool Valideer()
        {
            //
            return true;
        }
    }
}
