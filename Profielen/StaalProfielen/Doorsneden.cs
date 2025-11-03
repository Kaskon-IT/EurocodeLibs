namespace StaalProfielen
{
    public class Doorsneden
    {

        #region Database HE-profielen
        public readonly static ProfielIH HE100A = new() { Naam = "HE100A", H = 96, B = 100, Tw = 5, Tf = 8, R = 12 };
        public readonly static ProfielIH HE100B = new() { Naam = "HE100B", H = 100, B = 100, Tw = 6, Tf = 10, R = 12 };
        public readonly static ProfielIH HE100M = new() { Naam = "HE100M", H = 120, B = 106, Tw = 12, Tf = 20, R = 12 };
        public readonly static ProfielIH HE120A = new() { Naam = "HE120A", H = 114, B = 120, Tw = 5, Tf = 8, R = 12 };
        public readonly static ProfielIH HE120B = new() { Naam = "HE120B", H = 120, B = 120, Tw = 6.5, Tf = 11, R = 12 };
        public readonly static ProfielIH HE120M = new() { Naam = "HE120M", H = 140, B = 126, Tw = 12.5, Tf = 21, R = 12 };
        public readonly static ProfielIH HE140A = new() { Naam = "HE140A", H = 133, B = 140, Tw = 5.5, Tf = 8.5, R = 12 };
        public readonly static ProfielIH HE140B = new() { Naam = "HE140B", H = 140, B = 140, Tw = 7, Tf = 12, R = 12 };
        public readonly static ProfielIH HE140M = new() { Naam = "HE140M", H = 160, B = 146, Tw = 13, Tf = 22, R = 12 };
        public readonly static ProfielIH HE160A = new() { Naam = "HE160A", H = 152, B = 160, Tw = 6, Tf = 9, R = 15 };
        public readonly static ProfielIH HE160B = new() { Naam = "HE160B", H = 160, B = 160, Tw = 8, Tf = 13, R = 15 };
        public readonly static ProfielIH HE160M = new() { Naam = "HE160M", H = 180, B = 166, Tw = 14, Tf = 23, R = 15 };
        public readonly static ProfielIH HE180A = new() { Naam = "HE180A", H = 171, B = 180, Tw = 6, Tf = 9.5, R = 12 };
        public readonly static ProfielIH HE180B = new() { Naam = "HE180B", H = 180, B = 180, Tw = 8.5, Tf = 14, R = 12 };
        public readonly static ProfielIH HE180M = new() { Naam = "HE180M", H = 200, B = 186, Tw = 14.5, Tf = 24, R = 12 };
        public readonly static ProfielIH HE200A = new() { Naam = "HE200A", H = 190, B = 200, Tw = 6.50, Tf = 10.0, R = 18.0 };
        public readonly static ProfielIH HE200B = new() { Naam = "HE200B", H = 200, B = 200, Tw = 9.00, Tf = 15.0, R = 18.0 };
        public readonly static ProfielIH HE200M = new() { Naam = "HE200M", H = 220, B = 206, Tw = 15.0, Tf = 25.0, R = 18.0 };
        public readonly static ProfielIH HE220A = new() { Naam = "HE220A", H = 210, B = 220, Tw = 7.00, Tf = 11.0, R = 18.0 };
        public readonly static ProfielIH HE220B = new() { Naam = "HE220B", H = 220, B = 220, Tw = 9.50, Tf = 16.0, R = 18.0 };
        public readonly static ProfielIH HE220M = new() { Naam = "HE220M", H = 240, B = 226, Tw = 15.5, Tf = 26.0, R = 18.0 };
        public readonly static ProfielIH HE240A = new() { Naam = "HE240A", H = 230, B = 240, Tw = 5.50, Tf = 12.0, R = 21.0 };
        public readonly static ProfielIH HE240B = new() { Naam = "HE240B", H = 240, B = 240, Tw = 10.0, Tf = 17.0, R = 21.0 };
        public readonly static ProfielIH HE240M = new() { Naam = "HE240M", H = 270, B = 248, Tw = 18.0, Tf = 32.0, R = 21.0 };
        public readonly static ProfielIH HE260A = new() { Naam = "HE260A", H = 250, B = 260, Tw = 7.50, Tf = 12.5, R = 24.0 };
        public readonly static ProfielIH HE260B = new() { Naam = "HE260B", H = 260, B = 260, Tw = 10.0, Tf = 17.5, R = 24.0 };
        public readonly static ProfielIH HE260M = new() { Naam = "HE260M", H = 290, B = 268, Tw = 18.0, Tf = 32.5, R = 24.0 };
        public readonly static ProfielIH HE280A = new() { Naam = "HE280A", H = 270, B = 280, Tw = 8.00, Tf = 13.0, R = 24.0 };
        public readonly static ProfielIH HE280B = new() { Naam = "HE280B", H = 280, B = 280, Tw = 10.5, Tf = 18.0, R = 24.0 };
        public readonly static ProfielIH HE280M = new() { Naam = "HE280M", H = 310, B = 288, Tw = 18.5, Tf = 33.0, R = 24.0 };
        public readonly static ProfielIH HE300A = new() { Naam = "HE300A", H = 290, B = 300, Tw = 8.50, Tf = 14.0, R = 27.0 };
        public readonly static ProfielIH HE300B = new() { Naam = "HE300B", H = 300, B = 300, Tw = 11.0, Tf = 19.0, R = 27.0 };
        public readonly static ProfielIH HE300M = new() { Naam = "HE300M", H = 340, B = 310, Tw = 21.0, Tf = 39.0, R = 27.0 };
        public readonly static ProfielIH HE320A = new() { Naam = "HE320A", H = 310, B = 300, Tw = 9.00, Tf = 15.5, R = 27.0 };
        public readonly static ProfielIH HE320B = new() { Naam = "HE320B", H = 320, B = 300, Tw = 11.5, Tf = 20.5, R = 27.0 };
        public readonly static ProfielIH HE320M = new() { Naam = "HE320M", H = 359, B = 309, Tw = 21.0, Tf = 40.0, R = 27.0 };
        public readonly static ProfielIH HE340A = new() { Naam = "HE340A", H = 330, B = 300, Tw = 9.50, Tf = 16.5, R = 27.0 };
        public readonly static ProfielIH HE340B = new() { Naam = "HE340B", H = 340, B = 300, Tw = 12.0, Tf = 21.5, R = 27.0 };
        public readonly static ProfielIH HE340M = new() { Naam = "HE340M", H = 377, B = 309, Tw = 21.0, Tf = 40.0, R = 27.0 };
        public readonly static ProfielIH HE360A = new() { Naam = "HE360A", H = 350, B = 300, Tw = 10.0, Tf = 17.5, R = 27.0 };
        public readonly static ProfielIH HE360B = new() { Naam = "HE360B", H = 360, B = 300, Tw = 12.5, Tf = 22.5, R = 27.0 };
        public readonly static ProfielIH HE360M = new() { Naam = "HE360M", H = 395, B = 308, Tw = 21.0, Tf = 40.0, R = 27.0 };
        public readonly static ProfielIH HE400A = new() { Naam = "HE400A", H = 390, B = 300, Tw = 11.0, Tf = 19.0, R = 27.0 };
        public readonly static ProfielIH HE400B = new() { Naam = "HE400B", H = 400, B = 300, Tw = 13.5, Tf = 24.0, R = 27.0 };
        public readonly static ProfielIH HE400M = new() { Naam = "HE400M", H = 432, B = 307, Tw = 21.0, Tf = 40.0, R = 27.0 };
        public readonly static ProfielIH HE450A = new() { Naam = "HE450A", H = 440, B = 300, Tw = 11.5, Tf = 21.0, R = 27.0 };
        public readonly static ProfielIH HE450B = new() { Naam = "HE450B", H = 450, B = 300, Tw = 14.0, Tf = 26.0, R = 27.0 };
        public readonly static ProfielIH HE450M = new() { Naam = "HE450M", H = 478, B = 307, Tw = 21.0, Tf = 40.0, R = 27.0 };
        public readonly static ProfielIH HE500A = new() { Naam = "HE500A", H = 490, B = 300, Tw = 12.0, Tf = 23.0, R = 27.0 };
        public readonly static ProfielIH HE500B = new() { Naam = "HE500B", H = 500, B = 300, Tw = 14.5, Tf = 28.0, R = 27.0 };
        public readonly static ProfielIH HE500M = new() { Naam = "HE500M", H = 524, B = 306, Tw = 21.0, Tf = 40.0, R = 27.0 };
        public readonly static ProfielIH HE550A = new() { Naam = "HE550A", H = 540, B = 300, Tw = 12.5, Tf = 24.0, R = 27.0 };
        public readonly static ProfielIH HE550B = new() { Naam = "HE550B", H = 550, B = 300, Tw = 15.0, Tf = 29.0, R = 27.0 };
        public readonly static ProfielIH HE550M = new() { Naam = "HE550M", H = 572, B = 306, Tw = 21.0, Tf = 40.0, R = 27.0 };
        public readonly static ProfielIH HE600A = new() { Naam = "HE600A", H = 590, B = 300, Tw = 13.0, Tf = 25.0, R = 27.0 };
        public readonly static ProfielIH HE600B = new() { Naam = "HE600B", H = 600, B = 300, Tw = 15.5, Tf = 30.0, R = 27.0 };
        public readonly static ProfielIH HE600M = new() { Naam = "HE600M", H = 620, B = 305, Tw = 21.0, Tf = 40.0, R = 27.0 };
        public readonly static ProfielIH HE650A = new() { Naam = "HE650A", H = 640, B = 300, Tw = 13.5, Tf = 26.0, R = 27.0 };
        public readonly static ProfielIH HE650B = new() { Naam = "HE650B", H = 650, B = 300, Tw = 16.0, Tf = 31.0, R = 27.0 };
        public readonly static ProfielIH HE650M = new() { Naam = "HE650M", H = 668, B = 305, Tw = 21.0, Tf = 40.0, R = 27.0 };
        public readonly static ProfielIH HE700A = new() { Naam = "HE700A", H = 690, B = 300, Tw = 14.5, Tf = 27.0, R = 27.0 };
        public readonly static ProfielIH HE700B = new() { Naam = "HE700B", H = 700, B = 300, Tw = 17.0, Tf = 32.0, R = 27.0 };
        public readonly static ProfielIH HE700M = new() { Naam = "HE700M", H = 716, B = 304, Tw = 21.0, Tf = 40.0, R = 27.0 };
        public readonly static ProfielIH HE800A = new() { Naam = "HE800A", H = 790, B = 300, Tw = 15.0, Tf = 28.0, R = 30.0 };
        public readonly static ProfielIH HE800B = new() { Naam = "HE800B", H = 800, B = 300, Tw = 17.5, Tf = 33.0, R = 30.0 };
        public readonly static ProfielIH HE800M = new() { Naam = "HE800M", H = 814, B = 303, Tw = 21.0, Tf = 40.0, R = 30.0 };
        public readonly static ProfielIH HE900A = new() { Naam = "HE900A", H = 890, B = 300, Tw = 16.0, Tf = 30.0, R = 30.0 };
        public readonly static ProfielIH HE900B = new() { Naam = "HE900B", H = 900, B = 300, Tw = 18.5, Tf = 35.0, R = 30.0 };
        public readonly static ProfielIH HE900M = new() { Naam = "HE900M", H = 910, B = 302, Tw = 21.0, Tf = 40.0, R = 30.0 };
        public readonly static ProfielIH HE1000A = new() { Naam = "HE1000A", H = 990, B = 300, Tw = 16.5, Tf = 31.0, R = 30.0 };
        public readonly static ProfielIH HE1000B = new() { Naam = "HE1000B", H = 1000, B = 300, Tw = 19.0, Tf = 36.0, R = 30.0 };
        public readonly static ProfielIH HE1000M = new() { Naam = "HE1000M", H = 1008, B = 302, Tw = 21.0, Tf = 40.0, R = 30.0 };


        #endregion


        #region Database IPE-profielen

        public readonly static ProfielIH IPE80 = new() { Naam = "IPE80", H = 80, B = 46, Tw = 3.8, Tf = 5.2, R = 5 };
        public readonly static ProfielIH IPE100 = new() { Naam = "IPE100", H = 100, B = 55, Tw = 4.1, Tf = 5.7, R = 7 };
        public readonly static ProfielIH IPE120 = new() { Naam = "IPE120", H = 120, B = 64, Tw = 4.4, Tf = 6.3, R = 7 };
        public readonly static ProfielIH IPE140 = new() { Naam = "IPE140", H = 140, B = 73, Tw = 4.7, Tf = 6.9, R = 7 };
        public readonly static ProfielIH IPE160 = new() { Naam = "IPE160", H = 160, B = 82, Tw = 5.0, Tf = 7.4, R = 9 };
        public readonly static ProfielIH IPE180 = new() { Naam = "IPE180", H = 180, B = 91, Tw = 5.3, Tf = 8.0, R = 9 };
        public readonly static ProfielIH IPE200 = new() { Naam = "IPE200", H = 200, B = 100, Tw = 5.6, Tf = 8.5, R = 12 };
        public readonly static ProfielIH IPE220 = new() { Naam = "IPE220", H = 220, B = 110, Tw = 5.9, Tf = 9.2, R = 12 };
        public readonly static ProfielIH IPE240 = new() { Naam = "IPE240", H = 240, B = 120, Tw = 6.2, Tf = 9.8, R = 15 };
        public readonly static ProfielIH IPE270 = new() { Naam = "IPE270", H = 270, B = 135, Tw = 6.6, Tf = 10.2, R = 15 };
        public readonly static ProfielIH IPE300 = new() { Naam = "IPE300", H = 300, B = 150, Tw = 7.1, Tf = 10.7, R = 15 };
        public readonly static ProfielIH IPE330 = new() { Naam = "IPE330", H = 330, B = 160, Tw = 7.5, Tf = 11.5, R = 18 };
        public readonly static ProfielIH IPE360 = new() { Naam = "IPE360", H = 360, B = 170, Tw = 8.0, Tf = 12.7, R = 18 };
        public readonly static ProfielIH IPE400 = new() { Naam = "IPE400", H = 400, B = 180, Tw = 8.6, Tf = 13.5, R = 21 };
        public readonly static ProfielIH IPE450 = new() { Naam = "IPE450", H = 450, B = 190, Tw = 9.4, Tf = 14.6, R = 21 };
        public readonly static ProfielIH IPE500 = new() { Naam = "IPE500", H = 500, B = 200, Tw = 10.2, Tf = 16.0, R = 21 };
        public readonly static ProfielIH IPE550 = new() { Naam = "IPE550", H = 550, B = 210, Tw = 11.1, Tf = 17.2, R = 24 };
        public readonly static ProfielIH IPE600 = new() { Naam = "IPE600", H = 600, B = 220, Tw = 12.0, Tf = 19.0, R = 24 };

        public readonly static ProfielIH IPE750x134 = new() { Naam = "IPE750x134", H = 750, B = 264, Tw = 12.0, Tf = 15.5, R = 17 };
        public readonly static ProfielIH IPE750x147 = new() { Naam = "IPE750x147", H = 753, B = 265, Tw = 13.2, Tf = 17.0, R = 17 };

        #endregion


        /// <summary>
        /// Woordenboek voor HE-profielen.
        /// HEA, HEB en HEM profielen.
        /// (Bron: Arcelor Mittal)
        /// </summary>
        public readonly static Dictionary<string, ProfielIH> HE = new()
        {
            { HE100A.Naam, HE100A },
            { HE100B.Naam, HE100B },
            { HE100M.Naam, HE100M },
            { HE120A.Naam, HE120A },
            { HE120B.Naam, HE120B },
            { HE120M.Naam, HE120M },
            { HE140A.Naam, HE140A },
            { HE140B.Naam, HE140B },
            { HE140M.Naam, HE140M },
            { HE160A.Naam, HE160A },
            { HE160B.Naam, HE160B },
            { HE160M.Naam, HE160M },
            { HE180A.Naam, HE180A },
            { HE180B.Naam, HE180B },
            { HE180M.Naam, HE180M },
            { HE200A.Naam, HE200A },
            { HE200B.Naam, HE200B },
            { HE200M.Naam, HE200M },
            { HE220A.Naam, HE220A },
            { HE220B.Naam, HE220B },
            { HE220M.Naam, HE220M },
            { HE240A.Naam, HE240A },
            { HE240B.Naam, HE240B },
            { HE240M.Naam, HE240M },
            { HE260A.Naam, HE260A },
            { HE260B.Naam, HE260B },
            { HE260M.Naam, HE260M },
            { HE280A.Naam, HE280A },
            { HE280B.Naam, HE280B },
            { HE280M.Naam, HE280M },
            { HE300A.Naam, HE300A },
            { HE300B.Naam, HE300B },
            { HE300M.Naam, HE300M },
            { HE320A.Naam, HE320A },
            { HE320B.Naam, HE320B },
            { HE320M.Naam, HE320M },
            { HE340A.Naam, HE340A },
            { HE340B.Naam, HE340B },
            { HE340M.Naam, HE340M },
            { HE360A.Naam, HE360A },
            { HE360B.Naam, HE360B },
            { HE360M.Naam, HE360M },
            { HE400A.Naam, HE400A },
            { HE400B.Naam, HE400B },
            { HE400M.Naam, HE400M },
            { HE450A.Naam, HE450A },
            { HE450B.Naam, HE450B },
            { HE450M.Naam, HE450M },
            { HE500A.Naam, HE500A },
            { HE500B.Naam, HE500B },
            { HE500M.Naam, HE500M },
            { HE550A.Naam, HE550A },
            { HE550B.Naam, HE550B },
            { HE550M.Naam, HE550M },
            { HE600A.Naam, HE600A },
            { HE600B.Naam, HE600B },
            { HE600M.Naam, HE600M },
            { HE650A.Naam, HE650A },
            { HE650B.Naam, HE650B },
            { HE650M.Naam, HE650M },
            { HE700A.Naam, HE700A },
            { HE700B.Naam, HE700B },
            { HE700M.Naam, HE700M },
            { HE800A.Naam, HE800A },
            { HE800B.Naam, HE800B },
            { HE800M.Naam, HE800M },
            { HE900A.Naam, HE900A },
            { HE900B.Naam, HE900B },
            { HE900M.Naam, HE900M },
            { HE1000A.Naam, HE1000A },
            { HE1000B.Naam, HE1000B },
            { HE1000M.Naam, HE1000M },

        };


        /// <summary>
        /// Woordenboek voor IPE-profielen.
        /// (Bron: Arcelor Mittal)
        /// </summary>
        public readonly static Dictionary<string, ProfielIH> IPE = new Dictionary<string, ProfielIH>
        {
            { IPE80.Naam, IPE80 },
            { IPE100.Naam, IPE100 },
            { IPE120.Naam, IPE120 },
            { IPE140.Naam, IPE140 },
            { IPE160.Naam, IPE160 },
            { IPE180.Naam, IPE180 },
            { IPE200.Naam, IPE200 },
            { IPE220.Naam, IPE220 },
            { IPE240.Naam, IPE240 },
            { IPE270.Naam, IPE270 },
            { IPE300.Naam, IPE300 },
            { IPE330.Naam, IPE330 },
            { IPE360.Naam, IPE360 },
            { IPE400.Naam, IPE400 },
            { IPE450.Naam, IPE450 },
            { IPE500.Naam, IPE500 },
            { IPE550.Naam, IPE550 },
            { IPE600.Naam, IPE600 },
            { IPE750x134.Naam, IPE750x134 },
            { IPE750x147.Naam, IPE750x147 },

        };




    }



}
