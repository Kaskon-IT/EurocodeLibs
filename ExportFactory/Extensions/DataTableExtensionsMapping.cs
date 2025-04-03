using ExportFactory.Shared;
using K = CommonLibrary.EurocodeKeys;



namespace ExportFactory.Services
{


    public class DataTableMappingService
    {

        public Dictionary<string, AttributesMapping> Data { get; } = new();


        public DataTableMappingService()
        {
            Data = _algemeen
                .Concat(_ec0)
                .Concat(_ec1)
                .Concat(_ec2)
                .GroupBy(x => x.Key) // groepeer op de key (controle dubbele keys)
                .ToDictionary(g => g.Key, g => g.Last().Value); // behoud de laaste key indien dubbele gevonden


        }

        const string? _MPa = "0.##\tN/mm²";
        const string? _Promille = "0.##\t‰";
        // const string? _formatProcent = "0.##\t%";
        const string? _kN = "0.#\tkN";
        const string? _kNm = "0.#\tkNm";
        const string? _mm = "0\tmm";
        const string? _mmExact = "0.##\tmm";
        const string? _mm2 = "0\tmm²";
        const string? _formatVerhouding = "0.##"; // eenheidsloos
        const string? _graden = "0.##°"; // 


        // ALGEMEEN
        private readonly Dictionary<string, AttributesMapping> _algemeen = new()
        {
            { K.My, new(sym: "M~y~", desc:"Moment om Y-as") },
            { K.Mz, new(sym: "M~z~", desc:"Moment om Z-as") },

            { K.Vy, new(sym: "V~y~", desc:"Dwarskracht in Y-richting") },
            { K.Vz, new(sym: "V~z~", desc:"Dwarskracht in Z-richting") },

            { K.Tx, new(sym: "T~x~", desc:"Torsie om X-as") },
            { K.Nx, new(sym: "N~x~", desc:"Normaalkracht in X-as") },


            { K.Wapening, new(sym:"",desc:"wapening") },
            { K.WapeningToegepast, new(sym:"", desc:"wapening toegepast") },
            { K.WapeningVoorstel, new(sym:"", desc: "voorstel wapening") },



            { K.TrapOptredeMaat, new(sym:"a~optrede~", desc:"optrede maat")},
            { K.TrapAantredeMaat, new(sym:"a~aantrede~", desc:"aantrede maat")},
            { K.TrapOptredeAantal, new(sym:"n", desc:"aantal optreden")},
            { K.TrapSchilDikte, new(sym:"h~schil~", desc:"schildikte")},



            { K.MomentArm, new(sym: "a", desc:"arm voor moment") },
            { K.MomentRekenwaarde, new(sym: "M~Ed~", desc: "rekenwaarde moment") },
            { K.Staalspanning, new(sym: "|sigma|~s~", desc: "staalspanning") },

            { K.TandHoogte, new(sym: "h~tand~", desc:"hoogte van de tand")},
            { K.TandLengte, new(sym: "L~tand~", desc:"lengte van de tand")},
            { K.TandNuttigeHoogte, new(sym: "d~tand~", desc:"nuttige hoogte van de tand") },

            { K.ReactieRekenwaarde, new(sym: "F~Ed~", desc:"rekenwaarde oplegreactie") },
            { K.BelastingAfwerking, new(sym: "G~k,afw~", desc:"belasting uit afwerking") },



            { K.BetonDoorsnedeOppervlak, new(sym: "A~c~", desc:"betondoorsnedeoppervlak") },
            { K.ProfielBreedte, new(sym: "b", desc:"breedte profiel") },
            { K.ProfielHoogte, new(sym: "h", desc:"hoogte profiel") },
            { "B1", new(sym: "b~1~", desc:"afstand") },
            { "B2", new(sym: "b~2~", desc:"afstand") },
            { "H1", new(sym: "h~1~", desc:"afstand") },
            { "H2", new(sym: "h~1~", desc:"afstand") },


            { "Ix", new(sym: "I~x~", desc: "torsietraagheid (rond de x-as)") },
            { "Iy", new(sym: "I~y~", desc: "traagheidsmoment (rond de y-as)") },
            { "Iz", new(sym: "I~z~", desc: "traagheidsmoment (rond de z-as)") },

            { "Wy", new(sym: "W~y~", desc: "weerstandsmoment (rond de y-as)") },
            { "Wz", new(sym: "W~z~", desc: "weerstandsmoment (rond de z-as)") },

            { "ReferentieAfstandVoorNuttigeHoogte", new(sym: "h~ref~", desc:"referentie afstand voor nuttige hoogte") },
            { "XuD", new(sym: "x~u~/d", desc:"hoogte drukzone / nuttige hoogte") },
            { "NuttigeHoogte", new(sym: "d", desc:"nuttige hoogte") },


            {"AsBen", new(sym: "A~s,ben~", desc: "benodigde wapening") },
            {"AsToe", new(sym: "A~s,toe~", desc: "toegepaste wapening") },
            {"AsMin", new(sym: "A~s,min~", desc: "minimaal benodigde wapening") },
            {"Xu", new(sym: "x~u~", desc: "hoogte drukzone") },







        };


        // EC0
        private readonly Dictionary<string, AttributesMapping> _ec0 = new Dictionary<string, AttributesMapping>
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


        // EC1
        private readonly Dictionary<string, AttributesMapping> _ec1 = new()
        {

        };

        // EC2
        private readonly Dictionary<string, AttributesMapping> _ec2 = new Dictionary<string, AttributesMapping>
        {

            { "Betonsterkteklasse", new(sym : "C", desc : "betonsterkteklasse", norm: "EC2", art: "3.1.2", vgl: "tabel 3.1") },
            { "CementKlasse", new(sym: null, desc : "cement klasse", norm: "EC2", art : "3.1.2 (6)", vgl : null)},
            { "PoissonFactor", new(sym: "|nu|",desc: "poissonfactor", norm: "EC2", art : "3.1.3 (4)", vgl : null) },
            { "SpanningRekDiagram", new(sym: null, desc: "spanning-rekdiagram")},


            // B
            { "BetonStaalKwaliteit", new(sym : "B",desc : "sterkteklasse",norm : "EC2",art : "") },
            { "Betonstuik",         new(sym : "|epsilon|~c~",   desc : "betonstruik",                       norm : "EC2",   art : "3.1.3",          vgl : "tabel 3.1",      format: _Promille ) },
            { "BetonstuikGrens",    new(sym : "|epsilon|~cu~",  desc : "grenswaarde betonstruik",           norm : "EC2",   art : "3.1.3",          vgl : "tabel 3.1",      format: _Promille) },

            // C

            // D
            { "DekkingToe", new(sym: "c~toe~", desc:"toegepaste dekking") },
            { "DekkingNom", new(sym:"c~nom~",desc: "nominale dekking",norm:"EC2",art: "4.4.1.1") },
            { "DekkingMin", new(sym: "c~min~", desc : "minimale dekking",norm:"EC2", art: "4.4.1.2") },
            { "DekkingMinAanhechting", new(sym : "c~min,b~", desc : "minimale dekking aanhechting", norm : "EC2", art : "4.4.1.2 (3)", vgl:"tabel 4.2") },
            { "DekkingMinDuurzaamheid", new(sym : "c~min,dur~", desc : "minimale dekking duurzaamheid", norm : "EC2", art : "4.4.1.2 (5)", vgl : "tabel 4.4N") },
            { "DekkingToeslagUitvoeringsToleranties", new(sym : "|Delta|c~dev~", desc : "toeslag uitvoeringstoleranties", norm : "EC2", art : "4.4.1.3 (1)P") },


            // E
            { "EpsilonC",           new(sym : "|epsilon|~c~",   desc : "betonstruik",                               norm : "EC2",   art : "3.1.3",          vgl : "tabel 3.1",      format: _Promille) },
            { "EpsilonCu",          new(sym : "|epsilon|~cu~",  desc : "grenswaarde betonstruik",                   norm : "EC2",   art : "3.1.3",          vgl : "tabel 3.1",      format :_Promille) },
            { "Ecm",                new(sym: "E~cm~",           desc: "secans-elasticiteitsmodulus van beton",       norm: "EC2",art: "3.1.3",             vgl : "tabel 3.1",      format : _MPa) },
            { "Es",                 new(sym : "E~s~",           desc : "rekenwaarde elasticiteitsmodulus betonstaal", norm: "EC2", art: "3.2.7 (4)", vgl: "", format : _MPa) },

            // F
            { "Fck", new(sym: "f~ck~", desc: "karakteristieke cilinderdrukstertke",norm: "EC2", art: "3.1.2", vgl: "tabel 3.1" ) },
            { "FckCube", new(){Norm = "EC2" , Vergelijking = "tabel 3.1" , Article = "3.1.2", Description = "karakteristieke kubusdruksterkte", Symbol = "f~ck,cube~"} },
            { "Fcm", new(sym:"f~cm~",desc: "gemiddelde cilinderdruksterkte",norm:"EC2",art:"3.1.3", vgl: "tabel 3.1", format: _MPa) },

            { "Fctm", new(sym: "f~ctm~",desc:"gemiddelde axiale trekstrekste",norm: "EC2",art: "3.1.2",vgl: "tabel 3.1", format: _MPa) },
            { "FctmFl", new(sym: "f~ctm,fl~",desc : "gemiddelde buigtreksterkte", norm: "EC2", art: "3.1.8 (1)", vgl: "(3.23)", format : _MPa) },
            { "FctkVijfProcent", new(sym: "f~ctk,0,05~", desc:"karakteristieke axiale treksterkte 5% fractiel", norm: "EC2",art : "3.1.3", vgl: "tabel 3.1", format : _MPa) },
            { "FctkVijfEnNegentigProcent", new(sym: "f~ctk,0,95~",desc: "karakteristieke axiale treksterkte 95% fractiel", norm: "EC2", art: "3.1.3", vgl: "tabel 3.1", format: _MPa) },
            { "Fcd", new(sym: "f~cd~", desc:"rekenwaarde van de druksterkte",norm: "EC2", art:"3.1.6 (1)", vgl : "(3.15)",format: _MPa)},
            { "Fctd", new(sym: "f~ctd~",desc: "rekenwaarde van de treksterkte",norm: "EC2",art:  "3.1.6 (2)", vgl: "(3.16)",format :  _MPa)},

            { "Fyd", new(sym : "f~yd~",desc : "rekenwaarde sterkte betonstaal",norm: "EC2",art: "3.2",format:  _MPa) },
            { "Fywd", new(sym : "f~ywd~",desc : "rekenwaarde sterkte betonstaal (dwarskracht)",norm: "EC2",art: "3.2",format: _MPa) },
            { "Fyk", new(sym : "f~yk~",desc : "karakteristieke stekte betonstaal",norm:"EC2",art: "3.2.2 (1)P", vgl : "",format : _MPa) },
            
            // G
            { "GammaC",             new(sym : "|gamma|~C~",     desc : "partiële factor voor beton",        norm : "EC2",   art : "2.4.2.4 (1)",    vgl : "tabel 2.1N",     format : null) },
            { "GammaS",             new(sym : "|gamma|~S~",     desc : "partiële factor voor betonstaal",   norm : "EC2",   art : "2.4.2.4 (1)",    vgl : "tabel 2.1N",     format : null) },
            // K
            { "Kruipfactor",        new(sym : "|phi|(t,t~0~)",  desc : "kruipcoëfficiënt",                  norm : "EC2",   art : "Bijlage B.1",      vgl : "(B.1)",          format: null) },


            // Dekking en duurzaamheid
            { "MilieuklassenUserFriendlyName", new(sym : "X",desc : "milieuklasse(n)",norm:"EC2",art:"4.4") },

            { "IsPlaatGeometrie", new(sym: null, desc:"plaatgeometrie?", norm:"EC2", art:"4.4.1.2 (5)", vgl : "tabel 4.3N") },
            { "IsKwaliteitsBeheersing", new(sym : null, "kwaliteitsbeheersing?", norm : "EC2", art : "4.4.1.2 (5)", vgl: "tabel 4.3N") },
            { "GrootsteKorrelDiameter", new(sym : null, desc : "grootste korrel diameter", norm : "EC2", art : "4.4.1.2 (3)", vgl: "tabel 4.2") },
            { "ConstructieklasseUserFriendlyName", new(sym : "S", desc : "constructieklasse", norm : "EC2", art : "4.4.1.2 (5)", vgl : "tabel 4.3N")  },
            { "Naam", new(sym : null, desc : "", norm : null, art : null) }, // todo iets voor bedenken (universeel)
            { "WapeningDiameterGelijkwaardig", new(sym : "Ø~eq~", "gelijkwaardige diameter", norm : "EC2", art : "8.9.1") },


            // 6.2 Dwarskracht
            { K.DwarskrachtRekenwaarde, new(sym : "V~Ed~",desc : "rekenwaarde dwarskracht", norm : "", art:"" ,vgl: "", format : _kN) },

            { "Ved", new(sym : "V~Ed~",desc : "rekenwaarde dwarskracht", norm : "", art:"" ,vgl: "", format : _kN) },
            { K.DwarskrachtWeerstandBeton, new(sym : "V~Rd,c~", desc : "rekenwaarde dwarskracht opneembaar zonder dwarskrachtwapening", norm: "EC2" ,art : "6.2.1 (1)P\r\n6.2.2 (1)\r\n6.2.2 (2)", vgl: "(6.2)\r\n(6.4)", format : _kN) },
            { "DwarskrachtWeerstandStaal", new(sym : "V~Rd,s~", desc : "rekenwaarde dwarskracht opneembaar door dwarskrachtwapening",norm : "EC2", art :  "6.2.1 (1)P\r\n6.2.3 (3)" , vgl : "(6.8)"  , format : _kN) },
            { "DwarskrachtWeerstandMax", new(sym : "V~Rd,max~", desc : "rekenwaarde dwarskracht bovengrens bezwijken drukdiagonalen",norm : "EC2", art :  "6.2.1 (1)P", vgl : "(6.9)", format : _kN) },
            { "DwarskrachtWeerstand", new(sym : "V~Rd~",desc : "rekenwaarde opneembare dwarskracht", norm : "EC2", art:"6.2.1 (2)" ,vgl: "(6.1)", format : _kN) },

            { "SchuifspanningD", new(sym : "|nu|~Ed~",desc : "rekenwaarde schuifspanning", norm : "", art:"" ,vgl: "", format : _MPa) },
            { "SchuifspanningWeerstandBeton", new(sym : "|nu|~Rd,c~", desc : "rekenwaarde schuifspanning opneembaar zonder dwarskrachtwapening", norm: "EC2" ,art : "6.2.1 (1)P\r\n6.2.2 (1)\r\n6.2.2 (2)", vgl: "(6.2)\r\n(6.4)", format : _MPa) },
            { "SchuifspanningWeerstandStaal", new(sym : "|nu|~Rd,s~", desc : "rekenwaarde schuifspanning opneembaar door dwarskrachtwapening",norm : "EC2", art :  "6.2.1 (1)P\r\n6.2.3 (3)" , vgl : "(6.8)"  , format : _MPa) },
            { "SchuifspanningWeerstandMax", new(sym : "|nu|~Rd,max~", desc : "rekenwaarde schuifspanning bovengrens bezwijken drukdiagonalen",norm : "EC2", art :  "6.2.1 (1)P", vgl : "(6.9)", format : _MPa) },
            { "SchuifspanningWeerstand", new(sym : "|nu|~Rd~",desc : "rekenwaarde opneembare schuifspanning", norm : "EC2", art:"6.2.1 (2)" ,vgl: "(6.1)", format : _MPa) },



            { "ThetaHoekDrukdiagonaal", new(sym: "|theta|", desc: "hoek tussen drukdiagonaal en as van ligger loodrecht op de dwarskracht", norm : "EC2", art : "6.2.3 (1)\r\n6.2.3 (2)", vgl: "(6.7N)", format:_graden) },

            { "Alpha", new(sym:"|alpha|",  desc: "hoek dwarskrachtwapening", norm: "EC2", art:"6.2.3 (1)", vgl: "", format: _graden) },
            { "Theta", new(sym:"|theta|",  desc: "hoek drukdiagonaal", norm: "EC2", art:"6.2.3 (1)", vgl: "", format : _graden) },
            { "CotTheta", new(sym:"cot |theta|",  desc: "cotangens hoek drukdiagonaal", norm: "EC2", art:"6.2.3 (2)", vgl: "", format : _formatVerhouding) },

            { "AlphaCw", new(sym:"|alpha|~cw~",  desc: "factor spanning drukrand", norm: "EC2", art:"6.2.3 (3)", vgl: "(6.9)", format : _formatVerhouding) },
            { "NutHoogte", new(sym:"d",  desc: "nuttige hoogte voor dwarskracht", norm: "EC2", art:"6.2", vgl: "", format: _mm) },
            { "Breedte", new(sym:"b~w~",  desc: "minimale breedte tussen trek- en drukrand", norm: "EC2", art:"6.2.3 (1)", vgl: "", format : _mm) },
            { "Z", new(sym:"z",  desc: "inwendige hefboom", norm: "EC2", art:"6.2.3 (1)", vgl: "", format : _mm) },
            { "Asw", new(sym:"A~sw~",  desc: "doorsnedeoppervlak dwarskrachtwapening", norm: "EC2", art:"6.2.3 (3)", vgl: "(6.8)", format : _mm2) },
            { "AswMin", new(sym:"A~sw,min~",  desc: "minimale doorsnedeoppervlak dwarskrachtwapening", norm: "EC2", art:"6.2.3 (3)", vgl: "(6.8)", format: _mm2) },
            { "AswBerekend", new(sym:"A~sw~",  desc: "berekende doorsnedeoppervlak dwarskrachtwapening", norm: "EC2", art:"6.2.3 (3)", vgl: "(6.8)", format : _mm2) },
            { "AswBenPerMeter", new(sym:"A~sw,ben~",  desc: "benodigde doorsnedeoppervlak dwarskrachtwapening", norm: "EC2", art:"6.2.3 (3)", vgl: "(6.8)") },
            { "AswToegepast", new(sym:"A~sw,toe~",  desc: "toegepaste doorsnedeoppervlak dwarskrachtwapening", norm: "EC2", art:"", vgl: "") },

            { "AsLangs", new(sym:"A~sl~",  desc: "doorsnedeoppervlak trekwapening", norm: "EC2", art:"6.2.2 (1)", vgl: "(6.2)", format : _mm2) },
            { "Rho1", new(sym:"|rho|~1~",  desc: "= A~sl~ / (b~w~ d) ≤ 0,02", norm: "EC2", art:"6.2.2 (1)", vgl: "(6.2)", format : _formatVerhouding) },
            { "RhoWMin", new(sym:"|rho|~w,min~",  desc: "= (0,08 / √f~ck~) / f~yk~", norm: "EC2", art:"9.2.2 (5)", vgl: "(9.5N)", format : _formatVerhouding) },

            { "Crdc", new(sym:"C~Rd,c~",  desc: "factor", norm: "EC2", art:"6.2.2 (1)", vgl: "(6.2)", format : _formatVerhouding) },
            { "SterkteReductieFactorBetonGescheurdDoorDwarskracht", new(sym: "|nu|", desc: "sterktereductiefactor beton gescheurd door dwarskracht", norm: "EC2", art: "6.2.2 (6)", vgl:"(6.6N)", format : _formatVerhouding ) },
            { "SterkteReductieFactorBetonGescheurdDoorDwarskracht1", new(sym: "|nu|~1~", desc: "sterktereductiefactor beton gescheurd door dwarskracht", norm: "EC2", art: "6.2.3 (3)", vgl:"(6.9)", format : _formatVerhouding ) },

            { "FactorKDwarskrachtWeerstandBeton", new(sym:"k",  desc: "factor", norm: "EC2", art:"6.2.2 (1)", vgl: "(6.2)", format : _formatVerhouding) },
            { "FactorK1DwarskrachtWeerstandBeton", new(sym:"k~1~",  desc: "factor", norm: "EC2", art:"6.2.2 (1)", vgl: "(6.2)", format : _formatVerhouding) },



            { "HohAfstandBeugels", new(sym : "s", desc : "hart-op-hartafstand van de beugels", norm : "EC2", art : "", format : _mm) },
            //{ "Fywd", new(sym : "f~ywd~", desc : "rekenwaarde van de vloeigrens van de dwarskrachtwapening", norm : "EC2", art : "6.2.3 (3)", format : _formatSpanning) },
            { "Key", new(sym : "", desc : "", norm : "EC2", art : "", vgl: null, format : "") },
           

            // 7.3.2 Oppervlaktes van de minimumwapening
            // 7.3.4 Scheurwijdte
            // 
            { "SrMax", new(sym: "s~r,max~",desc: "maximale scheurafstand", norm:"EC2", art: "7.3.4", vgl: "(7.11)\r\n(7.14)\r\n(7.15)",format: "0.##" )},
            { K.MomentScheurmoment, new("M~cr~", "scheurmoment", "", _kNm, "") },
            { K.MomentFrequent, new(sym: "M~E,freq~", desc : "moment frequente combinatie", norm: "", art : "", format: _kNm) },
            //{ K.MomentRekenwaarde, new(sym: "M~Ed~", desc : "moment rekenwaarde", norm: "", art : "", format: _kNm) },

            { "EpsSmMinusEpsCm", new(sym:"|epsilon|~sm~ - |epsilon|~cm~", desc:"gemiddelde rek wapening minus gemiddelde betonrek",norm:"EC2",art:"7.3.4 (2)",vgl: "(7.9)", format:"e2")  },
            { "FactorKt", new(sym: "k~t~",desc:  "factor belastingsduur",norm:"EC2",art: "7.3.4 (2)", format: _formatVerhouding) },
            { "StaalspanningOptredend", new(sym: "|sigma|~s~", desc: "spanning trekwapening",norm: "EC2",art: "7.3.4 (2)",format: _MPa)},
            { "RhoPeff", new(sym: "|rho|~p,eff~",desc: "= (A~s~ + |xi|~1~ A~p~')/A~c,eff~", norm: "EC2", art: "7.3.4 (2)" , vgl: "(7.10)", format : _formatVerhouding) },
            { "ScheurwijdteVerhoudingElasticiteitsmodulusStaalBeton", new(sym : "|alpha|~e~", desc : "verhouding E~s~ / E~cm~", norm : "EC2", art : "7.3.4 (2)", format : _formatVerhouding) },
            { K.ScheurwijdteBerekend, new(sym: "w~k~", desc: "berekende scheurwijdte",norm: "EC2", art:"7.3.4 (1)",vgl: "(7.8)",format: _mmExact) },
            { K.ScheurwijdteMax, new(sym:"w~max~",desc: "grenswaarde scheurwijdte", norm: "EC2", art: "7.3.1 (5)", format: _mmExact) },
            { K.ScheurwijdteKx, new(sym:"k~x~",desc: "factor voor w~max~", norm: "EC2", art: "7.3.1 (5)", vgl:"", format: _formatVerhouding) },
            { K.ScheurwijdteK1, new(sym: "k~1~", desc: "factor aanhechtingseigenschappen", norm:"EC2", art: "7.3.4 (3)", vgl:"", format: _formatVerhouding) },
            { K.ScheurwijdteK2, new(sym : "k~2~", desc : "factor rekverdeling", norm : "EC2", art : "7.3.4 (3)",vgl:"", format: _formatVerhouding) },
            { K.ScheurwijdteK3, new(sym : "k~3~", desc : "factor zie nationale bijlage", norm : "EC2", art : "7.3.4 (3)", vgl : "(7.11)", format : _formatVerhouding) },
            { K.ScheurwijdteK4, new(sym : "k~4~", desc : "factor zie nationale bijlage", norm : "EC2", art : "7.3.4 (3)", vgl : "(7.11)", format : _formatVerhouding) },
            { K.ScheurwijdteAsMin, new(sym:"A~s,min~", desc: "minimale wapening",norm:"EC2", art: "7.3.1", vgl: "", format: _mm2) },
            { "WapeningToegepastTekst", new(sym:"A~s,toe~", desc: "toegepaste wapening",norm:"", art: "", vgl: "", format: null) },


            { "e", new() },


            { "BijlageB1", new(sym : "|phi|(t,t~0~)", desc : "kruipcoëfficiënt", norm : "EC2", art : "Bijlage B.1", vgl: "(B.1)", format : "") },
            { "BijlageB2", new(sym : "|phi|~0~", desc : "theoretische kruipcoëfficiënt", norm : "EC2", art : "Bijlage B.1", vgl: "(B.2)", format : "") },
            { "BijlageB3", new(sym : "|phi|~RH~", desc : "factor relatieve vochtigheid", norm : "EC2", art : "Bijlage B.1", vgl: "(B.3)", format : "") },
            { "BijlageB4", new(sym : "|beta|(f~cm~)", desc : "factor betonsterkte", norm : "EC2", art : "Bijlage B.1", vgl: "(B.4)", format : "") },
            { "BijlageB5", new(sym : "|beta|(t~0~)", desc : "factor ouderdom beton", norm : "EC2", art : "Bijlage B.1", vgl: "(B.5)", format : "") },

            { "BijlageB6", new(sym : "h~0~", desc : "theoretische dikte", norm : "EC2", art : "Bijlage B.1", vgl: "(B.6)", format : "") },
            { "BijlageB7", new(sym : "|beta|~c~(t,t~0~)", desc : "coëfficiënt ontwikkeling kruip in de tijd na belasten", norm : "EC2", art : "Bijlage B.1", vgl: "(B.7)", format : "") },
            { "BijlageB8", new(sym : "", desc : "", norm : "EC2", art : "Bijlage B.1", vgl: "(B.8)", format : "") },
            { "BijlageB9", new(sym : "t~0~", desc : "ouderdom in dagen", norm : "EC2", art : "Bijlage B.1", vgl: "(B.9)", format : "0\tdagen") },
            { "BijlageB10", new(sym : "t~T~", desc : "voor temperatuur gecorrigeerde ouderdom van het beton", norm : "EC2", art : "Bijlage B.1", vgl: "(B.10)", format : "0\tdagen") },

            { "BijlageB11", new(sym : "|epsilon|~cd,0~", desc : "basisverkorting ten gevolge van uitdrogingskrimp", norm : "EC2", art : "Bijlage B.2", vgl: "(B.11)", format : "") },
            { "BijlageB12", new(sym : "|beta|~RH~", desc : "factor relatieve vochtigheid", norm : "EC2", art : "Bijlage B.2", vgl: "(B.12)", format : "") },



            // Oplegging
            { "OplegLengteNominaal", new(sym: "a", desc: "nominale opleglengte a = a~1~ + a~2~ + a~3~ + √(|Delta|a~2~^2^ + |Delta|a~3~^2^) + |Delta|~e~", norm:"EC2", art: "10.9.5.2", vgl: "(10.6)" , format: _mm )  },
            { "OplegLengteNetto", new(sym: "a~1~", desc: "netto-opleglengte mbt oplegspanning a1 = F~Ed~ / (b~1~ f~Rd~), maar mag niet kleiner zijn dan de minimumwaarde in tabel 10.2", norm:"EC2", art: "10.9.5.2", vgl: "", format: _mm  )  },
            { "OplegLengteAanwezig", new(sym: "a~aanw~", desc: "aanwezige opleglengte", norm:"EC2", art: "10.9.5.2", vgl: "", format: _mm  )  },
            { "OplegReactieRekenwaarde", new(sym: "F~Ed~", desc: "rekenwaarde oplegreactie", norm:"EC2", art: "10.9.5.2", vgl: "(10.6)", format: _kN  )  },
            { "OplegBreedteNetto", new(sym: "b~1~", desc: "netto-oplegbreedte", norm:"EC2", art: "10.9.5.2", vgl: "(10.6)", format: _mm  )  },
            { "OplegSterkteRekenwaarde", new(sym: "f~Rd~", desc: "rekenwaarde oplegsterkte", norm:"EC2", art: "10.9.5.2", vgl: "(10.6)", format: _MPa  )  },

            { "AfstandA2", new(sym: "a~2~", desc: "randafstand dragende element", norm:"EC2", art: "10.9.5.2", vgl: "(10.6)", format: _mm  )  },
            { "AfstandA3", new(sym: "a~3~", desc: "randafstand ondersteunde element", norm:"EC2", art: "10.9.5.2", vgl: "(10.6)", format: _mm  )  },
            { "AfstandDeltaA2", new(sym: "|Delta|a~2~", desc: "tolerantie afstand tussen dragende elementen", norm:"EC2", art: "10.9.5.2", vgl: "(10.6)", format: _mm  )  },
            { "AfstandDeltaA3", new(sym: "|Delta|a~3~", desc: "tolerantie lengte element Δ~a3~ = l~n~/2500", norm:"EC2", art: "10.9.5.2", vgl: "(10.6)", format: _mm  )  },
            { "AfstandDeltaElementType", new(sym: "|Delta|~e~", desc: "indien afzonderlijk element, nominale lengte +20 mm.", norm:"EC2", art: "10.9.5.3", vgl: "", format: _mm  )  },

            { "LengteOndersteundeElement", new(sym: "l~n~", desc: "lengte ondersteunde element", norm:"EC2", art: "10.9.5.2", vgl: "(10.6)", format: _mm  )  },

            { "RelatieveOplegspanning", new(sym: "|sigma|~Ed~ / f~cd~", desc: "relatieve oplegspanning", norm:"EC2", art: "10.9.5.2", vgl: "Tabel 10.2/3", format: _formatVerhouding  )  },
            { "OplegSpanningRekenwaarde", new(sym: "|sigma|~Ed~", desc: "oplegspanning", norm:"EC2", art: "10.9.5.2", vgl: "", format: _MPa  )  },


            { "RekenwaardeOplegmateriaal", new(sym: "f~bed~", desc: "rekenwaarde oplegmateriaal", norm:"EC2", art: "10.9.5.2 (2)", vgl: "", format: _MPa  )  },
            { "LaagsteRekenwaardeVanOndersteundeEnHetOndersteunendeElement", new(sym: "f~cd~", desc: "laagste rekenwaarde van de sterktes van het ondersteunde en het ondersteunende element", norm:"EC2", art: "10.9.5.2 (2)", vgl: "", format: _MPa  )  },
            { "OpleggingElementType", new(sym: "", desc: "doorgaand of afzonderlijk", norm:"EC2", art: "10.9.5.2/3", vgl: "", format: _MPa  )  },
            { "OpgaveDruksterkteMetselwerk", new(sym: "f~bd~", desc: "rekenwaarde druksterkte metselwerk (volgens EN771)", norm: "", art:"", vgl:"", format: _MPa) },







        };


        //public Dictionary<string, AttributesMapping> GetEurocodeMapping()
        //{
        //    return _ec0.Concat(_ec2).ToDictionary();
        //}
        //_mappingEurocode0
        //.Concat(_mappingEurcode2)
        //.ToDictionary();



    }
}
