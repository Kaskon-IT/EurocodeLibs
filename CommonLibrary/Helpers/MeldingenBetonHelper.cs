namespace CommonLibrary.Helpers
{
    public class MeldingenBetonHelper
    {


        public static readonly Dictionary<int, Melding> MeldingenBeton = new()
        {
            // Vul de lijst met vooraf ingestelde meldingen
            // Dit is noodzakelijk voor herkenning (zelfde nummer, zelfde melding)
            // en voor het tonen van de melding in de UI
            // aangezien er meerdere regels zijn die dezelfde melding kunnen genereren

            // meldingen serie 1001 zijn voor moment
            // in het algemeen begin met opmerkingen
            // gebruik de hogere nummers voor waarschuwingen

            // NB. Gebruik HTML, we halen deze opmerkingen niet door de markdown parser!
            // NB2. Gebruik geen dynamische tekst, want kan op meerdere regels van toepassing zijn!

            { 1001, new Melding(MeldingType.Opmerking, "[1] Minimale wapening toegepast conform artikel 9.2.1.1 (1)") },
            { 1002, new Melding(MeldingType.Opmerking, "[2] Minimale wapening tbv gecontroleerde scheurvorming toegepast conform artikel 7.3.2") },
            { 1003, new Melding(MeldingType.Opmerking, "[3] Wapening op basis van gedrongen ligger conform artikel 6.1 (10)") },
            { 1004, new Melding(MeldingType.Opmerking, "[4] Het opneembare moment M<sub>Rd</sub> conform gedrongen ligger artikel 6.1(10) is groter dan M<sub>Rd</sub> volgens 6.1(P). Het maatgevende artikel 6.1(P) is toegepast.")},

            { 1051, new Melding(MeldingType.Waarschuwing, "[51] <u>Overschrijding maximale drukzone</u>") },
            { 1052, new Melding(MeldingType.Waarschuwing, "[52] <u>Overschrijding maximale wapening</u>") },
            { 1053, new Melding(MeldingType.Waarschuwing, "[53] <u>Wapening voldoet niet (uiterste grenstoestand)</u>") },


            

            // meldingen serie 2001 
            { 2004, new Melding(MeldingType.Opmerking, "[4] Voor de berekening van V<sub>Rd,max</sub> is meer dwarskrachtwapening toegepast zodat spanning kleiner is dan 80% van f<sub>yk</sub> , |nu|~1~ is bepaald met (vgl. 6.10N)" ) },
            { 2051, new Melding(MeldingType.Waarschuwing, "[51] <u>Overschrijding V<sub>Rd,max</sub> conform artikel 6.2.3 (3).</u> Pas de drukdiagonaal aan.")},


            // meldingen serie 3001 zijn voor dekking

            { 3051, new Melding(MeldingType.Waarschuwing, "[51] <u>Toegepaste dekking is kleiner dan nominale dekking</u>") },
            
            
            // meldingen serie 4001 zijn voor scheurwijdte
            
            { 4051, new Melding(MeldingType.Waarschuwing, "[51] <u>Berekende scheurwijdte w<sub>k</sub> (art. 7.3.4) groter dan toelaatbaar w<sub>max</sub> (art. 7.3.1) </u>")},



        };

        public static Melding GetMelding(int code)
        {
            return MeldingenBeton.TryGetValue(code, out var melding) ? melding : new Melding(MeldingType.Waarschuwing, bericht: "[?] Onbekende melding");
        }




    }
}
