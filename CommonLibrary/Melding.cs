namespace CommonLibrary
{

    public enum MeldingType
    {
        Opmerking,
        Waarschuwing,
        Foutmelding,
    }

    public class Melding
    {
        public MeldingType Type { get; set; }
        public string Bericht { get; set; }


        public Melding(MeldingType type, string bericht)
        {
            Type = type;
            Bericht = bericht;
        }

        public override string ToString()
        {
            return $"{Type}:{Bericht}";
        }

    }
}
