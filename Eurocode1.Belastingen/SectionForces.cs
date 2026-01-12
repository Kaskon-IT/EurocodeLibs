using CommonLibrary;
using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace Eurocode.Belastingen
{
    public static class SectionForcesHelper
    {
        public static SectionForcesEnvelope MaakOmhulling(IEnumerable<SectionForces> forces)
        {
            double minTx = double.PositiveInfinity, maxTx = double.NegativeInfinity;
            double minMy = double.PositiveInfinity, maxMy = double.NegativeInfinity;
            double minMz = double.PositiveInfinity, maxMz = double.NegativeInfinity;
            double minNx = double.PositiveInfinity, maxNx = double.NegativeInfinity;
            double minVy = double.PositiveInfinity, maxVy = double.NegativeInfinity;
            double minVz = double.PositiveInfinity, maxVz = double.NegativeInfinity;

            foreach (var f in forces)
            {
                minTx = Math.Min(minTx, f.Tx); maxTx = Math.Max(maxTx, f.Tx);
                minMy = Math.Min(minMy, f.My); maxMy = Math.Max(maxMy, f.My);
                minMz = Math.Min(minMz, f.Mz); maxMz = Math.Max(maxMz, f.Mz);
                minNx = Math.Min(minNx, f.Nx); maxNx = Math.Max(maxNx, f.Nx);
                minVy = Math.Min(minVy, f.Vy); maxVy = Math.Max(maxVy, f.Vy);
                minVz = Math.Min(minVz, f.Vz); maxVz = Math.Max(maxVz, f.Vz);
            }

            return new SectionForcesEnvelope(
                new SectionForcesSnapshot(minTx, minMy, minMz, minNx, minVy, minVz) ,
                new SectionForcesSnapshot(maxTx, maxMy, maxMz, maxNx, maxVy, maxVz)
            );
        }
    }

    public record SectionForcesEnvelopePerType(BelastingCombinatieTypeEnum Type, SectionForcesEnvelope Envelope);

   





    public record SectionForcesSnapshot(double Tx, double My, double Mz, double Nx, double Vy, double Vz)
    {
        public static SectionForcesSnapshot From(SectionForces f) =>
            new(f.Tx, f.My, f.Mz, f.Nx, f.Vy, f.Vz);
    }


    public sealed record SectionForcesEntry(double Tx, double My, double Mz, double Nx,  double Vy,  double Vz);


    public record SectionForcesEnvelope(SectionForcesSnapshot Min, SectionForcesSnapshot Max)
    {
        public static SectionForcesEnvelope From(IEnumerable<SectionForcesEntry> entries)
        {
            double minTx = double.PositiveInfinity, maxTx = double.NegativeInfinity;
            double minMy = double.PositiveInfinity, maxMy = double.NegativeInfinity;
            double minMz = double.PositiveInfinity, maxMz = double.NegativeInfinity;
            double minNx = double.PositiveInfinity, maxNx = double.NegativeInfinity;
            double minVy = double.PositiveInfinity, maxVy = double.NegativeInfinity;
            double minVz = double.PositiveInfinity, maxVz = double.NegativeInfinity;

            foreach (var entry in entries)
            {
                minTx = Math.Min(minTx, entry.Tx);
                maxTx = Math.Max(maxTx, entry.Tx);

                minMy = Math.Min(minMy, entry.My);
                maxMy = Math.Max(maxMy, entry.My);

                minMz = Math.Min(minMz, entry.Mz);
                maxMz = Math.Max(maxMz, entry.Mz);

                minNx = Math.Min(minNx, entry.Nx);
                maxNx = Math.Max(maxNx, entry.Nx);

                minVy = Math.Min(minVy, entry.Vy);
                maxVy = Math.Max(maxVy, entry.Vy);

                minVz = Math.Min(minVz, entry.Vz);
                maxVz = Math.Max(maxVz, entry.Vz);
            }

            return new SectionForcesEnvelope(
                new SectionForcesSnapshot(minTx, minMy, minMz, minNx, minVy, minVz),
                new SectionForcesSnapshot(maxTx, maxMy, maxMz, maxNx, maxVy, maxVz)
            );
        }
    }




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
                    AddMeldingError($"Overschrijding maximale waarde ({name}: {obj})");
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
