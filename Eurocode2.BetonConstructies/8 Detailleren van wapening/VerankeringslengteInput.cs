namespace Eurocode.BetonConstructies
{
    using CommonLibrary.Models;

    /// <summary>
    /// Type staaf voor de verankering. Bepaalt onder andere de minimum
    /// verankeringslengte (8.6)/(8.7) en de toepasbare α-factoren (Tabel 8.2).
    /// </summary>
    public enum VerankeringStaafType
    {
        Trekstaaf = 1,
        Drukstaaf = 2,
    }

    /// <summary>
    /// Vorm van de staaf. Een gebogen staaf (haak, lus of buiging) geeft een
    /// gunstiger <i>α</i><sub>1</sub>/<i>α</i><sub>2</sub>, maar vereist een
    /// minimale buigrol- respectievelijk buigstraal volgens §8.3.
    /// </summary>
    public enum VerankeringStaafVorm
    {
        Recht = 1,
        Gebogen = 2,
    }

    /// <summary>
    /// Invoer voor de berekening van de verankeringslengte van wapening volgens EC2 §8.3 en §8.4.
    /// Herbruikbaar, onder andere als aanvulling op de J3-console berekening.
    /// </summary>
    public class VerankeringslengteInput : BaseInput
    {
        private BetonContext _beton = new();
        /// <summary> Betoncontext (levert onder andere <i>f</i><sub>ctd</sub> en <i>f</i><sub>cd</sub>). </summary>
        public BetonContext Beton
        {
            get => _beton;
            set => SetProperty(ref _beton, value);
        }

        private double _diameter = 16;
        /// <summary> Ø – staafdiameter [mm]. </summary>
        public double Diameter
        {
            get => _diameter;
            set => SetProperty(ref _diameter, value);
        }

        private double _fyk = 500;
        /// <summary> <i>f</i><sub>yk</sub> – karakteristieke vloeigrens betonstaal [N/mm²]. </summary>
        public double Fyk
        {
            get => _fyk;
            set => SetProperty(ref _fyk, value);
        }

        private double _gammaS = 1.15;
        /// <summary> <i>γ</i><sub>s</sub> – partiële factor betonstaal. </summary>
        public double GammaS
        {
            get => _gammaS;
            set => SetProperty(ref _gammaS, value);
        }

        private double _benuttingsgraad = 1.0;
        /// <summary> Benuttingsgraad van de staaf (<i>A</i><sub>s,req</sub>/<i>A</i><sub>s,prov</sub>), 0..1. </summary>
        public double Benuttingsgraad
        {
            get => _benuttingsgraad;
            set => SetProperty(ref _benuttingsgraad, value);
        }

        /// <summary> <i>f</i><sub>yd</sub> = <i>f</i><sub>yk</sub> / <i>γ</i><sub>s</sub>. </summary>
        public double Fyd => Fyk / GammaS;

        /// <summary>
        /// <i>σ</i><sub>sd</sub> – rekenwaarde van de staafspanning ter plaatse van de verankering [N/mm²].
        /// </summary>
        public double RekenwaardeStaafspanning => Fyd * Benuttingsgraad;

        private VerankeringStaafType _staafType = VerankeringStaafType.Trekstaaf;
        /// <summary> Type staaf (trek- of drukstaaf). </summary>
        public VerankeringStaafType StaafType
        {
            get => _staafType;
            set => SetProperty(ref _staafType, value);
        }

        private VerankeringStaafVorm _staafVorm = VerankeringStaafVorm.Recht;
        /// <summary> Vorm van de staaf (recht of gebogen/haak/lus). </summary>
        public VerankeringStaafVorm StaafVorm
        {
            get => _staafVorm;
            set => SetProperty(ref _staafVorm, value);
        }

        private bool _goedeAanhechting = true;
        /// <summary> Goede aanhechtingsomstandigheden (<i>η</i><sub>1</sub> = 1,0), anders 0,7. </summary>
        public bool GoedeAanhechting
        {
            get => _goedeAanhechting;
            set => SetProperty(ref _goedeAanhechting, value);
        }

        private double _cd = 35;
        /// <summary> <i>c</i><sub>d</sub> – dekkingsmaat volgens figuur 8.3 (min. van <i>a</i>/2, <i>c</i><sub>1</sub> en <i>c</i>) [mm]. </summary>
        public double Cd
        {
            get => _cd;
            set => SetProperty(ref _cd, value);
        }

        private double _alpha3 = 1.0;
        /// <summary> <i>α</i><sub>3</sub> – opsluiting door (niet gelaste) dwarswapening. Standaard 1,0. </summary>
        public double Alpha3
        {
            get => _alpha3;
            set => SetProperty(ref _alpha3, value);
        }

        private double _alpha4 = 1.0;
        /// <summary> <i>α</i><sub>4</sub> – opsluiting door gelaste dwarswapening. Standaard 1,0. </summary>
        public double Alpha4
        {
            get => _alpha4;
            set => SetProperty(ref _alpha4, value);
        }

        private double _alpha5 = 1.0;
        /// <summary> <i>α</i><sub>5</sub> – opsluiting door dwarsdruk. Standaard 1,0. </summary>
        public double Alpha5
        {
            get => _alpha5;
            set => SetProperty(ref _alpha5, value);
        }

        private double _fbt = 0;
        /// <summary>
        /// <i>F</i><sub>bt</sub> – trekkracht in de staaf aan het begin van de buiging [N].
        /// 0 = de controle op betondrukbezwijken bij de buiging (8.1) overslaan.
        /// </summary>
        public double Fbt
        {
            get => _fbt;
            set => SetProperty(ref _fbt, value);
        }

        private double _ab = 0;
        /// <summary>
        /// <i>a</i><sub>b</sub> – halve hart-op-hart afstand van de staven, of (dekking + Ø/2)
        /// voor een randstaaf, in het vlak van de buiging [mm].
        /// </summary>
        public double Ab
        {
            get => _ab;
            set => SetProperty(ref _ab, value);
        }

        private double _toegepasteVerankeringslengte = 0;
        /// <summary> Optioneel: toegepaste/beschikbare verankeringslengte [mm] voor een unity check. </summary>
        public double ToegepasteVerankeringslengte
        {
            get => _toegepasteVerankeringslengte;
            set => SetProperty(ref _toegepasteVerankeringslengte, value);
        }
    }
}
