using ExportFactory.Shared;

namespace ExportFactory.Extensions
{
    public static partial class DataTableExtensions
    {

        const string? _formatSpanning = "0.##\tN/mm²";
        const string? _formatPromille = "0.##\t‰";
        const string? _formatProcent = "0.##\t%";
        const string? _formatVerhouding = null;




        private static readonly Dictionary<string, AttributesMapping> _propertieMap = new Dictionary<string, AttributesMapping>
        {

            // Gebruikte normen
            { "GebruikteNorm", new(null, "gebruikte norm") },




            // Grondslagen
            { "Kfi", new("K~FI~", "belastingfactor tbv de betrouwbaarheidsdifferentiatie", "")},
            { "Xi", new("|xi|", "reductiefactor voor ongunstige blijvende belastingen", "") },
            { "Gevolgklasse", new("CC", "gevolgklasse (Consequence Class)") },
            { "Betrouwbaarheidsklasse", new("RC", "betrouwbaarheidsklasse (Reliability Class)") },
            { "OntwerpLevensduur", new(null, "Ontwerplevensduur") },
            { "FlagEmoji", new(null, "Land") },
            { "FlagSvg", new(null, "Nationale Bijlage") },

            // Betonconstructies
            // c = concrete
            // k = karakteristiek
            // m = gemiddelde

            { "Betonstuik", new("|epsilon|~c~", "betonstruik") },
            { "BetonstuikGrens", new("|epsilon|~cu~", "grenswaarde betonstruik") },



            { "Fck", new(){Article = "3.1", Description = "karakteristieke cilinderdruksterkte", Symbol = "f~ck~"} },
            { "FckCube", new(){Article = "3.1", Description = "karakteristieke kubusdruksterkte", Symbol = "f~ck,cube~"} },
            { "Fcm", new(symbol:"f~cm~", description: "gemiddelde cilinderdruksterkte" ) },
            { "Ecm", new("E~cm~", "secans-elasticiteitsmodulus van beton") },
            { "Fctm", new("f~ctm~", "gemiddelde axiale trekstrekste") },
            { "Betonsterkteklasse", new( "C", "betonsterkteklasse", "tabel 3.1") },
            { "CementKlasse", new( null, "cement klasse", "art.")},
            { "PoissonFactor", new( "|nu|", "poissonfactor") },
            { "SpanningRekDiagram", new(null, "spanning-rekdiagram")},
            { "FctkVijfProcent", new("f~ctk,0,05~", "karakteristieke axiale treksterkte 5% fractiel", null, _formatSpanning) },
            { "FctkVijfEnNegentigProcent", new("f~ctk,0,95~", "karakteristiek", null, _formatSpanning) },
            { "Fcd", new("f~cd~", "rekenwaarde van de druksterkte", null, _formatSpanning)},
            { "Fctd", new("f~ctd~", "rekenwaarde van de treksterkte", null, _formatSpanning)},



            // Griekse kleine letters
            { "EpsilonC", new("|epsilon|~c~", "betonstruik", null, _formatPromille) },
            { "EpsilonCu", new("|epsilon|~cu~", "grenswaarde betonstruik", null, _formatPromille) },
            { "GammaC", new("|gamma|~C~", "partiële factor voor beton") },
            { "GammaS", new("|gamma|~S~", "partiële factor voor betonstaal") },

            { "Kruipfactor", new("|phi|(t,t~0~)", "kruipcoëfficiënt", "B.1") },



            // Betonstaal
            { "Sterkteklasse", new("B", "sterkteklasse") },


            // Dekking en duurzaamheid
            { "MilieuklassenUserFriendlyName", new("X", "milieuklasse(n)") },
            { "DekkingNom", new("c~nom~", "nominale dekking", "4.4.1.1") },
            { "DekkingMin", new("c~min~", "minimale dekking", "4.4.1.2") },
            { "DekkingMinAanhechting", new("c~min,b~", "minimale dekking aanhechting", "4.4.1.2(3)") },
            { "DekkingMinDuurzaamheid", new("c~min,dur~", "minimale dekking duurzaamheid", "4.4.1.2(5)") },
            { "DekkingToeslagUitvoeringsToleranties", new("|Delta|c~dev~", "toeslag uitvoeringstoleranties", "4.4.1.3") },
            { "IsPlaatGeometrie", new(null, "plaatgeometrie?") },
            { "IsKwaliteitsBeheersing", new(null, "kwaliteitsbeheersing?") },
            { "GrootsteKorrelDiameter", new(null, "grootste korrel diameter") },
            { "ConstructieklasseUserFriendlyName", new("S", "constructieklasse")  },
            { "Naam", new( null, "") }, // todo iets voor bedenken (universeel)
            { "WapeningDiameterGelijkwaardig", new("Ø~eq~", "gelijkwaardige diameter") },


        };

    }
}
