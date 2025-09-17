using CommonLibrary.Interfaces;
using Microsoft.AspNetCore.Components;

namespace CommonLibrary
{
    public class DubbeleWaarde
    {
        public bool IsZero => Ed == 0 && Kar == 0;

        public double Ed { get; set; }
        public double Kar { get; set; }

        public DubbeleWaarde()
        {
            Ed = 0;
            Kar = 0;
        }

        public DubbeleWaarde(double ed, double kar)
        {
            Ed = ed;
            Kar = kar;
        }

        // constructor voor enkele waarde opgave, wordt vertaalt naar een dubbelwaarde
        public DubbeleWaarde(double waarde) : this(waarde, waarde) { }



        public string ToString(string format, string prefix, string eenheid, string labelKar = ",kar~")
        {
            string labelEd = ",Ed~";

            if (Kar == Ed)
            {
                return $"{prefix}{labelEd} = {Ed.ToString(format)} {eenheid}";
            }
            else
            {
                return $"{prefix}{labelEd} = {Ed.ToString(format)} {eenheid}, {prefix}{labelKar} = {Kar.ToString(format)} {eenheid}";
            }

        }
    }





    public class Snedekrachten : IMarkupConvertible
    {

        public Snedekrachten(DubbeleWaarde my = default, DubbeleWaarde mz = default,
            DubbeleWaarde vy = default, DubbeleWaarde vz = default,
            DubbeleWaarde nx = default, DubbeleWaarde tx = default)
        {
            My = my ?? new();
            Mz = mz ?? new();
            Vy = vy ?? new();
            Vz = vz ?? new();
            Nx = nx ?? new();
            Tx = tx ?? new();
        }

        // Static helper om verwarring te vermijden
        public static Snedekrachten FromDoubles(double my = 0, double mz = 0, double vy = 0, double vz = 0, double nx = 0, double tx = 0)
        {
            return new Snedekrachten(
                new DubbeleWaarde(my),
                new DubbeleWaarde(mz),
                new DubbeleWaarde(vy),
                new DubbeleWaarde(vz),
                new DubbeleWaarde(nx),
                new DubbeleWaarde(tx)
            );
        }

        public DubbeleWaarde My { get; set; }
        public DubbeleWaarde Mz { get; set; }
        public DubbeleWaarde Vy { get; set; }
        public DubbeleWaarde Vz { get; set; }
        public DubbeleWaarde Nx { get; set; }
        public DubbeleWaarde Tx { get; set; }

        public string LabelKar { get; set; } = ",freq~";

        public MarkupString ToMarkupString() => Helpers.MarkupHelper.ToMarkupString(ToString());

        public MarkupString ToMarkupString(string format) => Helpers.MarkupHelper.ToMarkupString(ToString(format));

        public string ToString(string format)
        {
            var result = new List<string>();
            if (!My.IsZero) result.Add($"{My.ToString(format, "M~y", "kNm", LabelKar)}");
            if (!Mz.IsZero) result.Add($"{Mz.ToString(format, "M~z", "kNm", LabelKar)}");
            if (!Vy.IsZero) result.Add($"{Vy.ToString(format, "V~y", "kN", LabelKar)}");
            if (!Vz.IsZero) result.Add($"{Vz.ToString(format, "V~z", "kN", LabelKar)}");
            if (!Nx.IsZero) result.Add($"{Nx.ToString(format, "N~x", "kN", LabelKar)}");
            if (!Tx.IsZero) result.Add($"{Tx.ToString(format, "T~x", "kNm", LabelKar)}");
            return string.Join(", ", result);
        }

        public override string ToString() => ToString("0.##");
    }




    public class SnedekrachtenDELETE(double my = 0, double mz = 0, double vy = 0, double vz = 0, double nx = 0, double tx = 0) : IMarkupConvertible
    {
        public double My { get; set; } = my;
        public double Mz { get; set; } = mz;
        public double Vy { get; set; } = vy;
        public double Vz { get; set; } = vz;
        public double Nx { get; set; } = nx;
        public double Tx { get; set; } = tx;

        public MarkupString ToMarkupString()
        {
            return Helpers.MarkupHelper.ToMarkupString(this.ToString());
        }

        public MarkupString ToMarkupString(string format)
        {
            return Helpers.MarkupHelper.ToMarkupString(this.ToString(format));
        }

        public string ToString(string format)
        {
            var result = "";
            if (My != 0) result += $"M~y~ = {My.ToString(format)} kNm, ";
            if (Vz != 0) result += $"V~z~ = {Vz.ToString(format)} kN, ";

            if (Mz != 0) result += $"M~z~ = {Mz.ToString(format)} kNm, ";
            if (Vy != 0) result += $"V~y~ = {Vy.ToString(format)} kN, ";

            if (Nx != 0) result += $"N~x~ = {Nx.ToString(format)} kN, ";
            if (Tx != 0) result += $"T~x~ = {Tx.ToString(format)} kNm, ";
            return result.TrimEnd(',', ' ');
        }

        public override string ToString()
        {
            return ToString("F2");
        }


    }
}
