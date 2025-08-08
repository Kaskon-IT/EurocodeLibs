using CommonLibrary.Helpers;
using CommonLibrary.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Eurocode.BetonConstructies
{
    public class Constructieklasse : IMarkupConvertible
    {
        public override string ToString()
        {
            return $"S{Klasse} = ({ConstructieKlasseBasis} + Δ~levensduur~ ({CorrectieLevensduur}) + Δ~beton~ ({CorrectieBeton}) + Δ~kwaliteit~ ({CorrectieKwaliteitsBeheersting}) + Δ~geometrie~ ({CorrectiePlaatGeometrie}))";
        }

        public MarkupString ToMarkupString()
        {
            return MarkupHelper.ToMarkupString(ToString());
        }

        public Constructieklasse(BetonDekkingContext dekking, BetonContext beton)
        {
            this.Initialiseer(dekking, beton);
        }


        public int CorrectieLevensduur { get; set; }
        public int CorrectieBeton { get; set; }
        public int CorrectiePlaatGeometrie { get; set; }
        public int CorrectieKwaliteitsBeheersting { get; set; }


        /// <summary>
        /// De basis voor de constructieklasse = 4
        /// </summary>
        public const int ConstructieKlasseBasis = 4;
        public int Klasse
        {
            get
            {
                return ConstructieKlasseBasis + CorrectieLevensduur + CorrectieBeton + CorrectieKwaliteitsBeheersting + CorrectiePlaatGeometrie;
            }
        }

        public string UserFriendlyName { get { return $"S{Klasse}"; } }





    }


}
