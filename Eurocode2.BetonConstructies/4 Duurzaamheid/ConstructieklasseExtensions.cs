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
            switch (dekking.Grondslagen.OntwerpLevensduur)
            {
                case Grondslagen.OntwerpLevensduurEnum.VijfEnZeventig:
                    constructieklasse.CorrectieLevensduur = 1;
                    break;
                case Grondslagen.OntwerpLevensduurEnum.Honderd:
                    constructieklasse.CorrectieLevensduur = 2;
                    break;
            }



            List<int> correctiesBeton = new List<int>();
            foreach (var mk in dekking.Milieuklassen)
            {
                switch (mk)
                {
                    case MilieuklasseEnum.X0:
                    case MilieuklasseEnum.XC1:
                        if (beton.Fck >= 30)
                            correctiesBeton.Add(-1);
                        else
                            correctiesBeton.Add(0);
                        break;
                    case MilieuklasseEnum.XC2:
                    case MilieuklasseEnum.XC3:
                        if (beton.Fck >= 35)
                            correctiesBeton.Add(-1);
                        else
                            correctiesBeton.Add(0);
                        break;
                    case MilieuklasseEnum.XC4:
                    case MilieuklasseEnum.XD1:
                    case MilieuklasseEnum.XD2:
                    case MilieuklasseEnum.XS1:
                        if (beton.Fck >= 40)
                            correctiesBeton.Add(-1);
                        else
                            correctiesBeton.Add(0);
                        break;
                    case MilieuklasseEnum.XD3:
                    case MilieuklasseEnum.XS2:
                    case MilieuklasseEnum.XS3:
                        if (beton.Fck >= 45)
                            correctiesBeton.Add(-1);
                        else
                            correctiesBeton.Add(0);
                        break;

                }
            }

            // correcties beton.. negatief getal dus grootste is maatgevend
            constructieklasse.CorrectieBeton += correctiesBeton.Max();

            if (dekking.IsPlaatGeometrie)
                constructieklasse.CorrectiePlaatGeometrie = -1;

            if (dekking.IsKwaliteitsBeheersing)
                constructieklasse.CorrectieKwaliteitsBeheersting = -1;

        }
    }


}
