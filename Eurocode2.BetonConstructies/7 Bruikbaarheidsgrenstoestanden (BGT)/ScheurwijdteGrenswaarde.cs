using CommonLibrary;
using Eurocode.Grondslagen;
using ExportFactory.Shared;
using System.Text.Json.Serialization;

namespace Eurocode.BetonConstructies
{
    public partial class Scheurbeheersing
    {
        //public override MarkupString ToHtml(bool isDraaiTabel = true)
        //{
        //    return this.ToHtml(isDraaiTabel);

        //    throw new NotImplementedException();
        //}

        public class ScheurwijdteGrenswaarde : BaseEurocodeContext
        {
            public ScheurwijdteGrenswaarde()
            {

            }
            public override string Heading { get; set; } = "Scheurwijdte grenswaarde";
            // 7.3      Scheurbeheersing
            // 7.3.1    Algemene beschouwingen
            // 7.3.1(5) Grenswaarde w,max 

            // constructores
            [JsonConstructor]
            public ScheurwijdteGrenswaarde(BetonDekkingContext dekkingEnduurzaamheid, NationaleBijlageEnum nationaleBijlage)
            {
                DekkingEnDuurzaamheid = dekkingEnduurzaamheid;
                NationaleBijlage = nationaleBijlage;
                Initialiseer();
            }



            public BetonDekkingContext DekkingEnDuurzaamheid { get; set; } = new();
            public NationaleBijlageEnum NationaleBijlage { get; set; } = NationaleBijlageEnum.EU;

            public ElementType ElementType { get; set; } = ElementType.Standaard;



            private double _factorKx = 1.0;

            [TableColumn(Label = "factor", Symbol = "<i>k</i><sub>x</sub>")]
            public double FactorKx
            {
                get { return _factorKx; }
            }
            public Formula FactorKxFormula => new() { 
                Name = "7.3.1 (5)", 
                StaticValue = "k_x = c_{prov} / c_{nom} \\leq 2",
                DynamicValue = $"= {DekkingEnDuurzaamheid.DekkingToe:0} / {DekkingEnDuurzaamheid.DekkingNom:0}"
            };



            private double _wMax = 0.10;

            [TableColumn(Label = "grenswaarde scheurwijdte", Symbol = "<i>w</i><sub>max</sub>")]
            public double Wmax
            {
                get
                {
                    return _wMax;
                }
            }



            public void Initialiseer()
            {
                SetFactorKx();
                SetScheurwijdteMax();

            }





            public void SetFactorKx()
            {
                switch (NationaleBijlage)
                {
                    default:
                    case Grondslagen.NationaleBijlageEnum.EU:
                        _factorKx = 1.0;
                        break;
                    case Grondslagen.NationaleBijlageEnum.NL:
                        _factorKx = Math.Min(2, DekkingEnDuurzaamheid.DekkingToe / DekkingEnDuurzaamheid.DekkingNom); // niet groter dan 2, dus math.min()
                        break;
                }
            }

            public void SetScheurwijdteMax()
            {
                _wMax = 0.40;
                foreach (MilieuklasseEnum mk in DekkingEnDuurzaamheid.Milieuklassen)
                {
                    var wmax = GetScheurwijdteMax(mk, ElementType, NationaleBijlage);
                    if (wmax < _wMax)
                    {
                        _wMax = wmax;
                    }
                }
                _wMax *= FactorKx;
            }


            public static double GetScheurwijdteMax(MilieuklasseEnum milieuklasse, ElementType elementType, NationaleBijlageEnum nationaleBijlage)
            {
                double returnVal = 0.40;
                switch (milieuklasse)
                {
                    default:
                    case MilieuklasseEnum.X0:
                    case MilieuklasseEnum.XC1:
                        switch (nationaleBijlage)
                        {
                            default:
                            case NationaleBijlageEnum.EU:
                                switch (elementType)
                                {
                                    case ElementType.Standaard: returnVal = 0.40; break;
                                    case ElementType.ElementenMetEenCombinatieVanBetonstaalEnVoorspanstaalMetAanhechting: returnVal = 0.20; break;
                                }
                                break;
                            case NationaleBijlageEnum.NL:
                                switch (elementType)
                                {
                                    case ElementType.Standaard: returnVal = 0.40; break;
                                    case ElementType.ElementenMetEenCombinatieVanBetonstaalEnVoorspanstaalMetAanhechting: returnVal = 0.30; break;
                                }
                                break;


                        }
                        break;
                    case MilieuklasseEnum.XC2:
                    case MilieuklasseEnum.XC3:
                    case MilieuklasseEnum.XC4:
                        switch (nationaleBijlage)
                        {
                            default:
                                switch (elementType)
                                {
                                    case ElementType.Standaard: returnVal = 0.30; break;
                                    case ElementType.ElementenMetEenCombinatieVanBetonstaalEnVoorspanstaalMetAanhechting: returnVal = 0.20; break;
                                }
                                break;
                        }
                        break;
                    case MilieuklasseEnum.XD1:
                    case MilieuklasseEnum.XD2:
                    case MilieuklasseEnum.XD3:
                    case MilieuklasseEnum.XS1:
                    case MilieuklasseEnum.XS2:
                    case MilieuklasseEnum.XS3:
                        switch (nationaleBijlage)
                        {
                            default:
                                switch (elementType)
                                {
                                    case ElementType.Standaard: returnVal = 0.20; break;
                                    case ElementType.ElementenMetEenCombinatieVanBetonstaalEnVoorspanstaalMetAanhechting: returnVal = 0.10; break;
                                }
                                break;
                        }
                        break;

                }

                return returnVal;
            }


            public override string ToString()
            {
                return $"w~max~ = {Wmax:0.##} mm (k~x~={DekkingEnDuurzaamheid.DekkingToe}/{DekkingEnDuurzaamheid.DekkingNom} ≤ 2 ={FactorKx:0.##})";
            }



            protected override void Bereken()
            {
                Initialiseer();
            }

            protected override bool Valideer()
            {
                return true;
            }

            //public override MarkupString ToHtml(bool isDraaiTabel = true)
            //{
            //    throw new NotImplementedException();
            //}
        }
    }
}
