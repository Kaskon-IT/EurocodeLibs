using CommonLibrary;
using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace Eurocode.Belastingen
{
    public class SectionForces : BaseEurocodeContext, INotifyPropertyChanged
    {
        // backing fields
        private double _my, _mz, _vy, _vz, _nx, _tx;


        [TableColumn(Label = "Moment (Y-as)", Symbol = "<i>M</i><sub>y</sub>", Unit = "kNm")]
        public double My
        {
            get => _my;
            set => SetProperty(ref _my, value);
        }

        //[TableColumn(Label = "Moment (Z-as)", Symbol = "<i>M</i><sub>z</sub>", Unit = "kNm")]
        public double Mz
        {
            get => _mz;
            set => SetProperty(ref _mz, value);
        }

        //[TableColumn(Label = "Dwarskracht (Y-as)", Symbol = "<i>V</i><sub>y</sub>", Unit = "kN")]
        public double Vy
        {
            get => _vy;
            set => SetProperty(ref _vy, value);

        }

        [TableColumn(Label = "Dwarskracht (Z-as)", Symbol = "<i>V</i><sub>z</sub>", Unit = "kN")]

        public double Vz
        {
            get => _vz;
            set => SetProperty(ref _vz, value);
        }

        //[TableColumn(Label = "Normaalkracht (X-as)", Symbol = "<i>N</i><sub>x</sub>", Unit = "kN")]
        public double Nx
        {
            get => _nx;
            set => SetProperty(ref _nx, value);
        }

        //[TableColumn(Label = "Torsie (X-as)", Symbol = "<i>T</i><sub>x</sub>", Unit = "kNm")]
        public double Tx
        {
            get => _tx;
            set => SetProperty(ref _tx, value);
        }


        private EenheidSysteem _eenheden = EenheidSysteem.KiloNewtonMeter;
        public EenheidSysteem Eenheden
        {
            get => _eenheden;
            set => SetProperty(ref _eenheden, value);
        }


        private BelastingCombinatieTypeEnum _combinatieType;
        public BelastingCombinatieTypeEnum CombinatieType
        {
            get => _combinatieType;
            set => SetProperty(ref _combinatieType, value);
        }




        private string Suffix
        {
            get
            {
                switch (_combinatieType)
                {
                    default:
                    case BelastingCombinatieTypeEnum.Fundamenteel_A:
                    case BelastingCombinatieTypeEnum.Fundamenteel_B:
                        return ",Ed";

                    case BelastingCombinatieTypeEnum.QuasiBlijvend:
                        return ",Eqb";

                    case BelastingCombinatieTypeEnum.Brand:
                        return ",Ebr";
                    case BelastingCombinatieTypeEnum.Frequent:
                        return ",Efr";
                    case BelastingCombinatieTypeEnum.Aardbeving:
                        return ",Eab";
                    case BelastingCombinatieTypeEnum.Karakteristiek:
                        return ",kar";
                    case BelastingCombinatieTypeEnum.Blijvend:
                        return ",bl";


                }

            }
        }



        public override string Heading { get; set; } = "Snedekrachten";

        public override string ToString()
        {
            return $"My: {My.ToEng()}, Mz: {Mz.ToEng()}, Vy: {Vy.ToEng()}, Vz: {Vz.ToEng()}, Nx: {Nx.ToEng()}, Tx: {Tx.ToEng()}";
        }



        public MarkupString GetMarkupString()
        {
            List<string> items = [];
            if (Tx != 0) items.Add($"<i>T</i><sub>x{Suffix}</sub>: {Tx.ToEng()}");
            if (My != 0) items.Add($"<i>M</i><sub>y{Suffix}</sub>: {My.ToEng()}");
            if (Mz != 0) items.Add($"<i>M</i><sub>z{Suffix}</sub>: {Mz.ToEng()}");

            if (Nx != 0) items.Add($"<i>N</i><sub>x{Suffix}</sub>: {Nx.ToEng()}");
            if (Vy != 0) items.Add($"<i>V</i><sub>y{Suffix}</sub>: {Vy.ToEng()}");
            if (Vz != 0) items.Add($"<i>V</i><sub>z{Suffix}</sub>: {Vz.ToEng()}");

            return new MarkupString(string.Join(", ", items));


        }

        protected override void Bereken()
        {
            // niks te berekenen hier...
        }

        protected override bool Valideer()
        {
            if (OverschrijdingMax(Math.Abs(My), MaxValue)) return false;
            if (OverschrijdingMax(Math.Abs(Vz), MaxValue)) return false;
            if (OverschrijdingMax(Math.Abs(Mz), MaxValue)) return false;
            if (OverschrijdingMax(Math.Abs(Vy), MaxValue)) return false;
            if (OverschrijdingMax(Math.Abs(Tx), MaxValue)) return false;
            if (OverschrijdingMax(Math.Abs(Nx), MaxValue)) return false;



            return true;
            //throw new NotImplementedException();
        }

        public bool OverschrijdingMax(object obj, double maxValue, string propertyName = null)
        {
            if (obj == null) return false;

            double numericValue;

            Type type = obj.GetType();


            // Check of het een numeriek type is
            if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte) ||
                type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort) ||
                type == typeof(float) || type == typeof(double) || type == typeof(decimal))
            {
                numericValue = Convert.ToDouble(obj);

                if (numericValue > maxValue)
                {
                    string name = propertyName ?? "waarde";
                    AddMeldingWaarschuwing($"Overschrijding maximale waarde ({name}: {obj})");
                    return true;
                }
            }

            return false;
        }

        const double MaxValue = 999999999;

        public SectionForces(
            double my = 0,
            double mz = 0,
            double vy = 0,
            double vz = 0,
            double nx = 0,
            double tx = 0,
            BelastingCombinatieTypeEnum combinatieType = BelastingCombinatieTypeEnum.Fundamenteel_A)
        {
            My = my;
            Mz = mz;
            Vy = vy;
            Vz = vz;
            Nx = nx;
            Tx = tx;
            CombinatieType = combinatieType;
        }

        // dynamische overrides voor Attribute.Symbol
        public string MySymbol => $"<i>M</i><sub>y{Suffix}</sub>";
        public string MzSymbol => $"<i>M</i><sub>z{Suffix}</sub>";
        public string VySymbol => $"<i>V</i><sub>y{Suffix}</sub>";
        public string VzSymbol => $"<i>V</i><sub>z{Suffix}</sub>";
        public string NxSymbol => $"<i>N</i><sub>x{Suffix}</sub>";
        public string TxSymbol => $"<i>T</i><sub>x{Suffix}</sub>";




    }
}
