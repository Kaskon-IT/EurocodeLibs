using Microsoft.AspNetCore.Components;

namespace CommonLibrary
{

    public enum MeldingType
    {
        Opmerking,
        Waarschuwing,
    }




    public class Melding
    {


        public const string colorWarning = "#E67E22"; // hexadecimale kleurcode voor waarschuwing 
        public const string colorInfo = "#17A2B8"; // typische kleur voor informatieve meldingen (blauwachtig)

        public MeldingType Type { get; set; }
        public string Bericht { get; set; }

        private string TypeEmoji
        {
            get
            {
                switch (Type)
                {
                    case MeldingType.Opmerking: return "ℹ️";
                    default:
                    case MeldingType.Waarschuwing: return "⚠️";
                }
            }
        }

        public string Css
        {
            get
            {
                switch (Type)
                {
                    case MeldingType.Opmerking: return "font-style: italic;";
                    default:
                    case MeldingType.Waarschuwing: return "color: var(--warning);";
                }
            }
        }

        public Melding(MeldingType type, string bericht)
        {
            Type = type;
            Bericht = bericht;
        }

        public override string ToString()
        {

            return $"{TypeEmoji} {Bericht}";
        }

        public MarkupString ToMarkupString()
        {
            var color = Type switch
            {
                MeldingType.Opmerking => colorInfo,
                _ => colorWarning, // MeldingType.Waarschuwing
            };

            // Omzetten van {kleur:tekst} naar <span style="color:kleur">tekst</span>
            string val = "{" + $"{color}:" + $"{this}" + "}";
            return new MarkupString(val);
        }


    }
}
