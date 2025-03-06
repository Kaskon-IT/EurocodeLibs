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



        // EC0
        private static readonly Dictionary<string, AttributesMapping> _mappings0 = new Dictionary<string, AttributesMapping>
        {
            { "GebruikteNorm", new(sym: null, desc: "gebruikte norm", norm: null, art: null ) },
            { "Gevolgklasse", new(sym:"CC", desc: "gevolgklasse (Consequence Class)", norm: "EC0", art:"2.3") },
            { "OntwerpLevensduur", new(sym: null, desc: "Ontwerplevensduur", norm:"EC0", art: "", vgl: "") },
            { "Kfi", new(sym : "K~FI~", desc : "belastingfactor tbv de betrouwbaarheidsdifferentiatie", norm: "EC0", art: "B.3.3", vgl: "tabel B3")},
            { "Xi", new(sym: "|xi|", desc: "reductiefactor voor ongunstige blijvende belastingen", norm:"EC0", art: "6.4.3.2 (3)", vgl: "(6.10b)") },
            { "Betrouwbaarheidsklasse", new(sym : "RC", desc : "betrouwbaarheidsklasse (Reliability Class)", norm : "EC0", art : "B.3.2") },
            { "FlagEmoji", new(sym: null, desc: "Land", norm: "", art: ""  ) },
            { "FlagSvg", new(null, "Nationale Bijlage") },

        };


        // EC2
        private static readonly Dictionary<string, AttributesMapping> _mapping2 = new Dictionary<string, AttributesMapping>
        {

            { "Betonsterkteklasse", new(sym : "C", desc : "betonsterkteklasse", norm: "EC2", art: "3.1.2", vgl: "tabel 3.1") },
            { "CementKlasse", new(sym: null, desc : "cement klasse", norm: "EC2", art : "3.1.2 (6)", vgl : null)},
            { "PoissonFactor", new(sym: "|nu|",desc: "poissonfactor", norm: "EC2", art : "3.1.3 (4)", vgl : null) },
            { "SpanningRekDiagram", new(sym: null, desc: "spanning-rekdiagram")},




            // B
            { "BetonStaalKwaliteit", new(sym : "B",desc : "sterkteklasse",norm : "EC2",art : "") },
            { "Betonstuik",         new(sym : "|epsilon|~c~",   desc : "betonstruik",                       norm : "EC2",   art : "3.1.3",          vgl : "tabel 3.1",      format: _formatPromille ) },
            { "BetonstuikGrens",    new(sym : "|epsilon|~cu~",  desc : "grenswaarde betonstruik",           norm : "EC2",   art : "3.1.3",          vgl : "tabel 3.1",      format: _formatPromille) },
            
            // C

            // D
            { "DekkingNom", new(sym:"c~nom~",desc: "nominale dekking",norm:"EC2",art: "4.4.1.1") },
            { "DekkingMin", new(sym: "c~min~", desc : "minimale dekking",norm:"EC2", art: "4.4.1.2") },
            { "DekkingMinAanhechting", new(sym : "c~min,b~", desc : "minimale dekking aanhechting", norm : "EC2", art : "4.4.1.2 (3)", vgl:"tabel 4.2") },
            { "DekkingMinDuurzaamheid", new(sym : "c~min,dur~", desc : "minimale dekking duurzaamheid", norm : "EC2", art : "4.4.1.2 (5)", vgl : "tabel 4.4N") },
            { "DekkingToeslagUitvoeringsToleranties", new(sym : "|Delta|c~dev~", desc : "toeslag uitvoeringstoleranties", norm : "EC2", art : "4.4.1.3 (1)P") },


            // E
            { "EpsilonC",           new(sym : "|epsilon|~c~",   desc : "betonstruik",                               norm : "EC2",   art : "3.1.3",          vgl : "tabel 3.1",      format: _formatPromille) },
            { "EpsilonCu",          new(sym : "|epsilon|~cu~",  desc : "grenswaarde betonstruik",                   norm : "EC2",   art : "3.1.3",          vgl : "tabel 3.1",      format :_formatPromille) },
            { "Ecm",                new(sym: "E~cm~",           desc: "secans-elasticiteitsmodulus van beton",       norm: "EC2",art: "3.1.3",             vgl : "tabel 3.1",      format : _formatSpanning) },
            { "Es",                 new(sym : "E~s~",           desc : "rekenwaarde elasticiteitsmodulus betonstaal", norm: "EC2", art: "3.2.7 (4)", vgl: "", format : _formatSpanning) },

            // F
            { "Fck", new(sym: "f~ck~", desc: "karakteristieke cilinderdrukstertke",norm: "EC2", art: "3.1.2", vgl: "tabel 3.1" ) },
            { "FckCube", new(){Norm = "EC2" , Vergelijking = "tabel 3.1" , Article = "3.1.2", Description = "karakteristieke kubusdruksterkte", Symbol = "f~ck,cube~"} },
            { "Fcm", new(sym:"f~cm~",desc: "gemiddelde cilinderdruksterkte",norm:"EC2",art:"3.1.3", vgl: "tabel 3.1", format: _formatSpanning) },

            { "Fctm", new(sym: "f~ctm~",desc:"gemiddelde axiale trekstrekste",norm: "EC2",art: "3.1.2",vgl: "tabel 3.1", format: _formatSpanning) },
            { "FctmFl", new(sym: "f~ctm,fl~",desc : "gemiddelde buigtreksterkte", norm: "EC2", art: "3.1.8 (1)", vgl: "(3.23)", format : _formatSpanning) },
            { "FctkVijfProcent", new(sym: "f~ctk,0,05~", desc:"karakteristieke axiale treksterkte 5% fractiel",art : "3.1.3", vgl: "tabel 3.1", format : _formatSpanning) },
            { "FctkVijfEnNegentigProcent", new(sym: "f~ctk,0,95~",desc: "karakteristiek", norm: "EC2", art: "3.1.3", vgl: "tabel 3.1", format: _formatSpanning) },
            { "Fcd", new(sym: "f~cd~", desc:"rekenwaarde van de druksterkte",norm: "EC2", art:"3.1.6 (1)", vgl : "(3.15)",format: _formatSpanning)},
            { "Fctd", new(sym: "f~ctd~",desc: "rekenwaarde van de treksterkte", "3.1.6 (2)", vgl: "(3.16)",format :  _formatSpanning)},

            { "Fyd", new(sym : "f~yd~",desc : "rekenwaarde sterkte betonstaal",norm: "EC2",art: "?",format:  _formatSpanning) },
            { "Fywd", new(sym : "f~ywd~",desc : "rekenwaarde sterkte betonstaal (dwarskracht)",norm: "EC2",art: "?",format: _formatSpanning) },
            { "Fyk", new(sym : "f~yk~",desc : "karakteristieke stekte betonstaal",norm:"EC2",art: "3.2.2 (1)P", vgl : "",format : _formatSpanning) },
            
            // G
            { "GammaC",             new(sym : "|gamma|~C~",     desc : "partiële factor voor beton",        norm : "EC2",   art : "2.4.2.4 (1)",    vgl : "tabel 2.1N",     format : null) },
            { "GammaS",             new(sym : "|gamma|~S~",     desc : "partiële factor voor betonstaal",   norm : "EC2",   art : "2.4.2.4 (1)",    vgl : "tabel 2.1N",     format : null) },
            // K
            { "Kruipfactor",        new(sym : "|phi|(t,t~0~)",  desc : "kruipcoëfficiënt",                  norm : "EC2",   art : "Bijlage B",      vgl : "(B.1)",          format: null) },


                                   
           
            


            // Dekking en duurzaamheid
            { "MilieuklassenUserFriendlyName", new(sym : "X",desc : "milieuklasse(n)",norm:"EC2",art:"4.4") },

            { "IsPlaatGeometrie", new(sym: null, desc:"plaatgeometrie?", norm:"EC2", art:"4.4.1.2 (5)", vgl : "tabel 4.3N") },
            { "IsKwaliteitsBeheersing", new(sym : null, "kwaliteitsbeheersing?", norm : "EC2", art : "4.4.1.2 (5)", vgl: "tabel 4.3N") },
            { "GrootsteKorrelDiameter", new(sym : null, desc : "grootste korrel diameter", norm : "EC2", art : "4.4.1.2 (3)", vgl: "tabel 4.2") },
            { "ConstructieklasseUserFriendlyName", new(sym : "S", desc : "constructieklasse", norm : "EC2", art : "4.4.1.2 (5)", vgl : "tabel 4.3N")  },
            { "Naam", new(sym : null, desc : "", norm : null, art : null) }, // todo iets voor bedenken (universeel)
            { "WapeningDiameterGelijkwaardig", new(sym : "Ø~eq~", "gelijkwaardige diameter", norm : "EC2", art : "?") },



            // 6.2 Dwarskracht

            { "DwarskrachtOpneembaarBeton", new(sym : "V~Rd,c~", desc : "rekenwaarde dwarskracht opneembaar zonder dwarskrachtwapening", norm: "EC2" ,art : "6.2.1 (1)P\r\n6.2.2 (1)\r\n6.2.2 (2)", vgl: "(6.2)\r\n(6.4)", format : _formatKracht) },
            { "DwarskrachtOpneembaarStaal", new(sym : "V~Rd,s~", desc : "rekenwaarde dwarskracht opneembaar door dwarskrachtwapening",norm : "EC2", art :  "6.2.1 (1)P\r\n6.2.3 (3)" , vgl : "(6.8)"  , format : _formatKracht) },
            { "DwarskrachtOpneembaarMax", new(sym : "V~Rd,max~", desc : "rekenwaarde dwarskracht bovengrens bezwijken drukdiagonalen",norm : "EC2", art :  "6.2.1 (1)P", vgl : "(6.9)", format : _formatKracht) },
            { "DwarskrachtOpneembaar", new(sym : "V~Rd~",desc : "rekenwaardopneembare krachtskracht", norm : "EC2", art:"6.2.1 (2)" ,vgl: "(6.1)", format : _formatKracht) },
            { "ThetaHoekDrukdiagonaal", new(sym: "|theta|", desc: "hoek tussen drukdiagonaal en as van ligger loodrecht op de dwarskracht", norm : "EC2", art : "6.2.3 (1)\r\n6.2.3 (2)", vgl: "(6.7N)") },

            { "HohAfstandBeugels", new(sym : "s", desc : "hart-op-hartafstand van de beugels", norm : "EC2", art : "", format : "0\tmm") },
            //{ "Fywd", new(sym : "f~ywd~", desc : "rekenwaarde van de vloeigrens van de dwarskrachtwapening", norm : "EC2", art : "6.2.3 (3)", format : _formatSpanning) },
            { "Key", new(sym : "", desc : "", norm : "EC2", art : "", vgl: null, format : "") },
           

            // 7.3.2 Oppervlaktes van de minimumwapening
            

            // 7.3.4 Scheurwijdte
            // 
            { "SrMax", new(sym: "s~r,max~",desc: "maximale scheurafstand", norm:"EC2", art: "7.3.4", vgl: "(7.11)\r\n(7.14)\r\n(7.15)",format: "0.##" )},
            { "Mcr", new("M~cr~", "scheurmoment", "", _formatMoment, "") },
            { "EpsSmMinusEpsCm", new(sym:"|epsilon|~sm~ - |epsilon|~cm~", desc:"gemiddelde rek wapening minus gemiddelde betonrek",norm:"EC2",art:"7.3.4 (2)",vgl: "(7.9)", format:"e2")  },
            { "FactorKt", new("k~t~", "factor belastingsduur", "7.3.4 (2)", _formatVerhouding) },
            { "StaalspanningOptredend", new("|sigma|~s~", "spanning trekwapening", "7.3.4 (2)", _formatSpanning)},
            { "RhoPeff", new(sym: "|rho|~p,eff~",desc: "= (A~s~ + |xi|~1~ A~p~')/A~c,eff~",art: "7.3.4 (2)" , vgl: "(7.10)", format : _formatVerhouding) },
            { "ScheurwijdteVerhoudingElasticiteitsmodulusStaalBeton", new("|alpha|~e~", "verhouding E~s~ / E~cm~", "7.3.4 (2)", _formatVerhouding) },
            { "Wk", new(sym: "w~k~", desc: "scheurwijdte",norm: "EC2", art:"",vgl: "(7.8)",format: "0.## mm") },
            { "ScheurwijdteMax", new(sym:"w~max~",desc: "grenswaarde scheurwijdte", norm: "EC2", art: "7.3.1 (5)", format: "0.## mm") },
            { "ScheurwijdteGrenswaardeFactorKx", new(sym:"k~x~",desc: "factor voor w~max~", norm: "EC2", art: "7.3.1 (5)", vgl:"", format: _formatVerhouding) },
            { "MaximaleScheurAfstandFactorK1", new(sym: "k~1~", desc: "factor aanhechtingseigenschappen", art: "7.3.4 (3)", vgl:"", format: _formatVerhouding) },
            { "MaximaleScheurAfstandFactorK2", new(sym : "k~2~", desc : "factor rekverdeling", art : "7.3.4 (3)",vgl:"", format: _formatVerhouding) },
            { "MaximaleScheurAfstandFactorK3", new(sym : "k~3~", desc : "factor zie nationale bijlage", art : "7.3.4 (3)", vgl : "(7.11)", format : _formatVerhouding) },
            { "MaximaleScheurAfstandFactorK4", new(sym : "k~4~", desc : "factor zie nationale bijlage", art : "7.3.4 (3)", vgl : "(7.11)", format : _formatVerhouding) },
            { "ScheurwijdteAsMin", new(sym:"A~s,min~", desc: "minimale wapening",norm:"EC2", art: "7.3.?", vgl: "", format:"0 mm²") },



            { "e", new() },


            { "BijlageB1", new(sym : "|sigma|(t,t~0~)", desc : "kruipcoëfficiënt", norm : "EC2", art : "Bijlage B", vgl: "(B.1)", format : "") },
            { "BijlageB2", new(sym : "|sigma|~0~", desc : "theoretische kruipcoëfficiënt", norm : "EC2", art : "Bijlage B", vgl: "(B.2)", format : "") },
            { "BijlageB3", new(sym : "|sigma|~RH~", desc : "factor relatieve vochtigheid", norm : "EC2", art : "Bijlage B", vgl: "(B.3)", format : "") },
            { "BijlageB4", new(sym : "|beta|(f~cm~)", desc : "factor betonsterkte", norm : "EC2", art : "Bijlage B", vgl: "(B.4)", format : "") },
            { "BijlageB5", new(sym : "|beta|(t~0~)", desc : "factor ouderdom beton", norm : "EC2", art : "Bijlage B", vgl: "(B.5)", format : "") },
            { "BijlageB6", new(sym : "h~0~", desc : "theoretische dikte", norm : "EC2", art : "Bijlage B", vgl: "(B.6)", format : "") },
            { "BijlageB7", new(sym : "|beta|~c~(t,t~0~)", desc : "", norm : "EC2", art : "Bijlage B", vgl: "(B.7)", format : "") },



            { "BijlageB8", new(sym : "", desc : "", norm : "EC2", art : "Bijlage B", vgl: "(B.8)", format : "") },

            { "BijlageB9", new(sym : "t~0~", desc : "", norm : "EC2", art : "Bijlage B", vgl: "(B.9)", format : "") },
            { "BijlageB10", new(sym : "t~T~", desc : "", norm : "EC2", art : "Bijlage B", vgl: "(B.10)", format : "") },
            { "BijlageB11", new(sym : "|epsilon|~cd,0~", desc : "", norm : "EC2", art : "Bijlage B", vgl: "(B.11)", format : "") },
            { "BijlageB12", new(sym : "|beta|~RH~", desc : "", norm : "EC2", art : "Bijlage B", vgl: "(B.12)", format : "") },



        };


        public static readonly Dictionary<string, AttributesMapping> EurocodeMapping =
            _mappings0
            .Concat(_mapping2)
            .ToDictionary();

        public static readonly Dictionary<string, AttributesMapping> _propertieMap = new Dictionary<string, AttributesMapping>
        {














        };

    }
}
