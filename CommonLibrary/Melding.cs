namespace CommonLibrary
{

    public enum MeldingType
    {
        Opmerking,
        Waarschuwing,
    }

    public class Melding
    {
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
                    case MeldingType.Waarschuwing: return "color: red;";
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



    }
}
