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

        private string Color
        {
            get
            {
                return Type switch
                {
                    MeldingType.Opmerking => colorInfo,
                    _ => colorWarning, // MeldingType.Waarschuwing
                };
            }
        }


        public MarkupString ToMarkupString()
        {
            //var color = Type switch
            //{
            //    MeldingType.Opmerking => colorInfo,
            //    _ => colorWarning, // MeldingType.Waarschuwing
            //};

            // Omzetten van {kleur:tekst} naar <span style="color:kleur">tekst</span>
            // ZET HET OM NAAR HTML dus gebruik geen Markdown 
            // gebruik een <span> element

            string val = $"<span style=color:{this.Color};>{this}</span>";

            return new MarkupString(val);
        }

        public string ToMarkDownString()
        {
            // Zet de melding om naar een Markdown string
            // Markdown gebruikt geen HTML, dus we gebruiken de standaard Markdown syntax
            // NB. we ondersteunen wel <sub><sup><strong><em><u><i><b> en <br> in de Markdown string
            // Alleen de "{#RRGGBB:gekleurde tekst}" wordt alleen ondersteund als MARKDOWN 
            string val = "{" + this.Color + ":" + this.ToString() + "}";



            return val;
        }


    }
}
