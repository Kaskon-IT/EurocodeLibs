using CommonLibrary;
using System.ComponentModel;

namespace Eurocode.BetonConstructies
{
    public partial class Scheurbeheersing : BaseEurocodeContext
    {
        public override string Heading { get; set; } = "Scheurbeheersing";
        public override bool IsAkkoord()
        {
            if (Meldingen.Any(m => m.Type == MeldingType.Waarschuwing)) return false;

            return true;
            throw new NotImplementedException();
        }

        public override string? ToString()
        {
            return base.ToString();
        }

        protected override void Bereken()
        {
            // wat moet er gebeuren?
            //throw new NotImplementedException();
        }

        protected override bool Valideer()
        {


            return true;
            //throw new NotImplementedException();
        }

        // 7.3 Scheurbeheersing
        public enum ElementType
        {
            [Description("Elementen met betonstaal en/of voorspanstaal **zonder** aanhechting")]
            Standaard = 1,
            [Description("Elementen met een combinatie van betonstaal en voorspanstaal MET aanhechting")]
            ElementenMetEenCombinatieVanBetonstaalEnVoorspanstaalMetAanhechting = 4,
            [Description("Elementen met uitsluitend voorspanstaal MET aanhechting")]
            ElementenMetUitsluitendVoorspanstaalMetAanhechting = 8,
        }

        public enum BelastingduurEnum { kortdurend = 1, langdurend = 2 };
    }
}
