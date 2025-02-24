using CommonLibrary;

namespace Eurocode.BetonConstructies.Algemeen
{
    class MeldingenBeton
    {
        public Dictionary<int, Melding> Meldingen { get; set; }

        public MeldingenBeton()
        {
            Meldingen = InitialiseerMeldingen();
        }


        private static Dictionary<int, Melding> InitialiseerMeldingen()
        {
            var meldingen = new Dictionary<int, Melding>();

            // Vul de lijst met vooraf ingestelde meldingen
            meldingen[201] = new Melding(MeldingType.Opmerking, "Minimale wapening toegepast conform artikel 9.2.1.1(1)");
            meldingen[202] = new Melding(MeldingType.Opmerking, "Gedrongen ligger toegepast conform artikel 6.1(10)");

            meldingen[203] = new Melding(MeldingType.Waarschuwing, "V~Ed~ is groter dan V~Rd,max~ conform artikel 6.2.3(3). Doorsnede niet akkoord");
            meldingen[217] = new Melding(MeldingType.Waarschuwing, "Toegepaste dekking is kleiner dan nominale dekking conform artikel 4.4.1.1");
            meldingen[262] = new Melding(MeldingType.Waarschuwing, "Berekende cheurwijdte w~k~ (7.3.4) groter dan toelaatbaar w~max~ (7.3.1)");
            meldingen[268] = new Melding(MeldingType.Opmerking, "M~Rd~ gedrongen ligger artikel 6.1(10) is groter dan M~Rd~ volgens 6.1(P). Artikel 6.1(P) is maatgevend en is toegepast.");

            return meldingen;
        }

    }
}
