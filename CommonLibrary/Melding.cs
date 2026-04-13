using Microsoft.AspNetCore.Components;

namespace CommonLibrary
{

    [Flags]
    public enum MeldingType
    {
        Opmerking = 1,
        Waarschuwing = 2,
        Neutraal = 4,
        Hint = 8,
        Error = 16,
    }




    public class Melding
    {

        public Guid Id { get; set; } = Guid.NewGuid();

        public int? Code { get; set; } // Optioneel, kan gebruikt worden voor catalogus

        public string GetCode
        {
            get
            {
                if (Code == null) return "";
                return (Code.Value % 1000).ToString();
            }
        }


        // kleuren voor meldingen
        public const string colorWarning = "#E67E22"; // hexadecimale kleurcode voor waarschuwing 
        public const string colorInfo = "#17A2B8"; // typische kleur voor informatieve meldingen (blauwachtig)
        public const string colorError = "#DC3545"; // hexadecimale kleurcode voor foutmelding (rood)
        public const string colorNeutral = "#6C757D"; // hexadecimale kleurcode voor neutrale meldingen (grijsachtig)
        public const string colorHint = "#28A745"; // hexadecimale kleurcode voor hint (groenachtig)

        public const string emojiWaarschuwing = "⚠️";
        public const string emojiHint = "💡";
        public const string emojiError = "❌";
        public const string emojiInfo = "ℹ️";

        public MeldingType Type { get; set; }
        public string Bericht { get; set; }
        public bool ShowEmoji { get; set; } = true; // Toon emoji in de UI, standaard aan


        private string BerichtHtml
        {
            get
            {
                if (Type == MeldingType.Waarschuwing)
                {
                    return $"{Bericht}";
                }
                else
                {
                    return Bericht;
                }
            }
        }

        public string GetEmoji
        {
            get
            {
                return Type switch
                {
                    MeldingType.Opmerking => emojiInfo,
                    MeldingType.Hint => emojiHint,
                    MeldingType.Error => emojiError,
                    MeldingType.Waarschuwing => emojiWaarschuwing,
                    _ => "",
                };
            }
        }

        private string? Emoji
        {
            get
            {
                if (ShowEmoji)
                {
                    return GetEmoji;
                }
                else
                {
                    return null; // Geen emoji tonen
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

        public Melding(MeldingType type, string bericht, int? code = null)
        {
            Type = type;
            Bericht = bericht;
            Code = code;
        }


        //public override string ToString() => Code.HasValue ? $"[{Code}] {Bericht}" : Bericht;
        public override string ToString()
        {
            List<string> parts = [];

            // code?
            if (Code.HasValue)
            {
                parts.Add($"[{Code % 1000}]"); // bijvoorbeeld 1. 
            }

            // bericht
            parts.Add(BerichtHtml);

            // emoji?
            if (Emoji != null)
            {
                parts.Add(Emoji);
            }

            // voorbeelden
            // 1. Lekker bezig hoor 💡
            // 51. Rode onderstreepte tekst !
            return string.Join(" ", parts);
        }



        private string Color
        {
            get
            {
                return Type switch
                {
                    MeldingType.Opmerking => colorInfo,
                    MeldingType.Error => colorError,
                    MeldingType.Neutraal => colorNeutral,
                    MeldingType.Hint => colorHint,
                    MeldingType.Waarschuwing => colorWarning,
                    _ => colorWarning, // Default case 
                };
            }
        }


        public MarkupString ToMarkupString()
        {
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

        public override bool Equals(object? obj)
        {
            if (obj is not Melding other)
                return false;

            if (Code.HasValue && other.Code.HasValue)
            {
                return Code.Value == other.Code.Value;
            }

            return string.Equals(Bericht, other.Bericht, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return Code.HasValue
                ? Code.Value.GetHashCode()
                : Bericht.GetHashCode();
        }


    }
}
