using ExportFactory.Shared;

namespace ExportFactory.Extensions
{
    public static partial class DataTableExtensions
    {

        const string? _formatSpanning = "0.##\tN/mm²";
        const string? _formatPromille = "0.##\t‰";
        const string? _formatProcent = "0.##\t%";
        const string? _formatKracht = "0.#\tkN";
        const string? _formatMoment = "0.#\tkNm";
        const string? _formatVerhouding = null;

        const string? _en1992 = "EN 1992";



        public static readonly Dictionary<string, AttributesMapping> _propertieMap = new Dictionary<string, AttributesMapping>
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

            { "Betonstuik", new("|epsilon|~c~", "betonstruik", "tabel 3.1", _formatPromille ) },
            { "BetonstuikGrens", new("|epsilon|~cu~", "grenswaarde betonstruik", "tabel 3.1", _formatPromille) },

            { "DwarskrachtOpneembaarBeton", new("V~Rd,c~", "rekenwaarde dwarskracht opneembaar zonder dwarskrachtwapening", "6.2.1 (1)P", _formatKracht) },
            { "DwarskrachtOpneembaarStaal", new("V~Rd,s~", "rekenwaarde dwarskracht opneembaar door dwarskrachtwapening", "6.2.1 (1)P" , _formatKracht) },
            { "DwarskrachtOpneembaarMax", new("V~Rd,max~", "rekenwaarde dwarskracht bovengrens bezwijken drukdiagonalen", "6.2.1 (1)P",_formatKracht) },
            { "DwarskrachtOpneembaar", new("V~Rd~", "rekenwaardopneembare krachtskracht", "(vgl 6.1)", _formatKracht) },








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
            { "BetonStaalKwaliteit", new("B", "sterkteklasse") },
            { "Fyd", new("f~yd~","rekenwaarde sterkte betonstaal", null, _formatSpanning) },
            { "Fywd", new("f~ywd~","rekenwaarde sterkte betonstaal (dwarskracht)", null, _formatSpanning) },
            { "Fyk", new("f~yk~","karakteristieke stekte betonstaal", null, _formatSpanning) },



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


            // 7.3.2 Oppervlaktes van de minimumwapening
            

            // 7.3.4 Scheurwijdte
            // 
            { "SrMax", new("s~r,max~", "maximale scheurafstand", "7.3.4", "0.##", _en1992 )},
            { "Mcr", new("M~cr~", "scheurmoment", "", _formatMoment, "") },
            { "EpsSmMinusEpsCm", new("|epsilon|~sm~ - |epsilon|~cm~", "gemiddelde rek wapening minus gemiddelde betonrek", "(7.9)", "e2", _en1992)  },
            { "FactorKt", new("k~t~", "factor belastingsduur", "7.3.4 (2)", _formatVerhouding, _en1992) },
            { "StaalspanningOptredend", new("|sigma|~s~", "spanning trekwapening", "7.3.4 (2)", _formatSpanning, _en1992)},
            { "RhoPeff", new("|rho|~p,eff~", "= (A~s~ + |xi|~1~ A~p~')/A~c,eff~", "(7.10)", _formatVerhouding, _en1992) },
            { "ScheurwijdteVerhoudingElasticiteitsmodulusStaalBeton", new("|alpha|~e~", "verhouding E~s~ / E~cm~", "7.3.4 (2)", _formatVerhouding, _en1992) },
            { "Wk", new("w~k~","scheurwijdte", "(7.8)", "0.## mm", _en1992) },
            { "ScheurwijdteMax", new("w~max~", "grenswaarde scheurwijdte", "7.3.1 (5)", "0.## mm", _en1992) },
            { "ScheurwijdteGrenswaardeFactorKx", new("k~x~", "factor voor w~max~", "7.3.1 (5)", _formatVerhouding, _en1992) },
            { "MaximaleScheurAfstandFactorK1", new("k~1~", "factor aanhechtingseigenschappen", "7.3.4 (3)", _formatVerhouding, _en1992) },
            { "MaximaleScheurAfstandFactorK2", new("k~2~", "factor rekverdeling", "7.3.4 (3)", _formatVerhouding, _en1992) },
            { "MaximaleScheurAfstandFactorK3", new("k~3~", "", "7.3.4 (3)", _formatVerhouding, _en1992) },
            { "MaximaleScheurAfstandFactorK4", new("k~4~", "", "7.3.4 (3)", _formatVerhouding, _en1992) },
            { "ScheurwijdteAsMin", new("A~s,min~", "minimale wapening", "7.3.?", "0 mm²", _en1992) },



            { "e", new() },





        };

    }
}
