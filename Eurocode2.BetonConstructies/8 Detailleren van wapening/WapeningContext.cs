namespace Eurocode.BetonConstructies
{
    public class WapeningContext
    {
        public WapeningContext()
        {

        }
        public WapeningContext(string tekst)
        {
            Tekst = tekst;
        }

        public string Tekst { get; set; } = "8-150";

        public double As { get { return WapeningHelper.GetDsnOpp(Tekst); } }

        private List<string>? _wapgroepen { get { return WapeningHelper.GetWapGroepen(Tekst); } }

        public double HohMaat { get { return WapeningHelper.GetKleinsteHohMaat(_wapgroepen); } }
    }
}
