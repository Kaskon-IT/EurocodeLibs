using CommonLibrary;

namespace Eurocode.BetonConstructies.Algemeen
{
    public class MeldingenBetonHelper
    {


        public static readonly Dictionary<int, Melding> MeldingenBeton = new()
        {
            // Vul de lijst met vooraf ingestelde meldingen
            { 201, new Melding(MeldingType.Opmerking, "Minimale wapening toegepast conform artikel 9.2.1.1 (1)") },
            { 202, new Melding(MeldingType.Opmerking, "Gedrongen ligger toegepast conform artikel 6.1 (10)")},
            { 203, new Melding(MeldingType.Waarschuwing, "V~Ed~ is groter dan V~Rd,max~ conform artikel 6.2.3 (3). Doorsnede niet akkoord. Pas de drukdiagonaal aan.")},
            { 204, new Melding(MeldingType.Opmerking, "Voor de berekening van V~Rd,max~ is meer dwarskrachtwapening (A~sw~) toegepast zodat spanning kleiner is dan 80% van f~yk~ , |nu|~1~ is bepaald met (vgl. 6.10N)" ) },

            { 217, new Melding(MeldingType.Waarschuwing, "Toegepaste dekking is kleiner dan nominale dekking conform artikel 4.4.1.1") },
            { 262, new Melding(MeldingType.Waarschuwing, "Berekende cheurwijdte w~k~ (7.3.4) groter dan toelaatbaar w~max~ (7.3.1)")},
            { 268, new Melding(MeldingType.Opmerking, "M~Rd~ gedrongen ligger artikel 6.1(10) is groter dan M~Rd~ volgens 6.1(P). Artikel 6.1(P) is maatgevend en is toegepast.")},



        };

        public static Melding GetMelding(int code)
        {
            return MeldingenBeton.TryGetValue(code, out var melding) ? melding : new Melding(MeldingType.Foutmelding, bericht: "Onbekende melding");
        }




    }
}
