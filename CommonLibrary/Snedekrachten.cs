using CommonLibrary.Interfaces;
using Microsoft.AspNetCore.Components;

namespace CommonLibrary
{
    public class Snedekrachten(double my = 0, double mz = 0, double vy = 0, double vz = 0, double nx = 0, double tx = 0) : IMarkupConvertible
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
            if (Mz != 0) result += $"M~z~ = {Mz.ToString(format)} kNm, ";
            if (Vy != 0) result += $"V~y~ = {Vy.ToString(format)} kN, ";
            if (Vz != 0) result += $"V~z~ = {Vz.ToString(format)} kN, ";
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
