namespace Eurocode.BetonConstructies
{
    public static class ConstructieklasseExtensions
    {
        /// <summary>
        /// Constructieve klasseficatie (Constructieklasse) volgens tabel 4.3N
        /// </summary>
        /// <param name="constructieklasse"></param>
        /// <param name="dekking"></param>5
        /// <param name="beton"></param>
        /// <returns></returns>
        public static void Initialiseer(this Constructieklasse constructieklasse, BetonDekkingContext dekking, BetonContext beton)
        {
            switch (dekking.OntwerpLevensduur)
            {
                case Grondslagen.OntwerpLevensduurEnum.VijfEnZeventig:
                    constructieklasse.CorrectieLevensduur = 1;
                    break;
                case Grondslagen.OntwerpLevensduurEnum.Honderd:
                    constructieklasse.CorrectieLevensduur = 2;
                    break;
            }

            if (beton.Fck >= 30)
                constructieklasse.CorrectieBeton = -1;

            if (dekking.IsPlaatGeometrie)
                constructieklasse.CorrectiePlaatGeometrie = -1;

            if (dekking.IsKwaliteitsBeheersing)
                constructieklasse.CorrectieKwaliteitsBeheersting = -1;

        }
    }


}
