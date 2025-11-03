using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eurocode.Belastingen
{
    public enum BelastingCombinatieTypeEnum
    {
        [Display(Name = "fundamenteel (6.10a)", ShortName = "fundamenteel")]
        [Description("fundamenteel (6.10a)")]
        Fundamenteel_A = 1001,

        [Display(Name = "fundamenteel (6.10b)", ShortName = "fundamenteel")]
        [Description("fundamenteel (6.10b)")]
        Fundamenteel_B = 1002,

        [Display(Name = "brand (6.11b)", ShortName = "brand")]
        [Description("brand (6.11b)")]
        Brand = 1101,

        [Display(Name = "aardbeving (6.12b)", ShortName = "aardbeving")]
        [Description("aardbeving (6.12b)")]
        Aardbeving = 1201,


        [Display(Name = "karakteristiek (6.14b)", ShortName = "karakteristiek")]
        [Description("karakteristiek (6.14b)")]
        Karakteristiek = 1401,

        [Display(Name = "frequent (6.15b)", ShortName = "frequent")]

        [Description("frequent (6.15b)")]
        Frequent = 1501,

        [Display(Name = "quasi-blijvend (6.16b)", ShortName = "quasi-blijvend")]
        [Description("quasi-blijvend (6.16b)")]
        QuasiBlijvend = 1601,

        [Display(Name = "blijvend", ShortName = "blijvend")]
        [Description("blijvend")]
        Blijvend = 9999,

    }


    public class BelastingenHelpers
    {
        public static string GetSubscript(BelastingCombinatieTypeEnum belastingCombinatieType)
        {
            switch (belastingCombinatieType)
            {
                default:
                case BelastingCombinatieTypeEnum.Fundamenteel_A:
                case BelastingCombinatieTypeEnum.Fundamenteel_B:
                    return "Ed";
                case BelastingCombinatieTypeEnum.Brand:
                    return "Efi";
                case BelastingCombinatieTypeEnum.Aardbeving:
                    return "Eeq";
                case BelastingCombinatieTypeEnum.Karakteristiek:
                    return "k";
                case BelastingCombinatieTypeEnum.Frequent:
                    return "Efr";
                case BelastingCombinatieTypeEnum.QuasiBlijvend:
                    return "Eqp";
                case BelastingCombinatieTypeEnum.Blijvend:
                    return "G";

            }
        }
    }






    public enum EenheidSysteem
    {
        NewtonMillimeter,   // basis (N, mm)
        KiloNewtonMeter,    // SI (kN, m)
        USImperial          // lb, inch
    }

    public enum Grootheid
    {
        Lengte,
        Kracht,
        Moment,
        Oppervlak,
        Volume,
        Weerstandsmoment,
        Traagheid,
        Spanning,
        Rek,
        Veerwaarde,
        Stijfheid,
        Kromming,

    }

    public record UnitDefinition(string Symbool, double FactorToBase);

    public static class UnitHelper
    {
        private static readonly Dictionary<(EenheidSysteem, Grootheid), UnitDefinition> _units =
            new()
            {
            // --- SI (N, mm) = basis ---
                { (EenheidSysteem.NewtonMillimeter, Grootheid.Lengte), new("mm",1.0) },
                { (EenheidSysteem.NewtonMillimeter, Grootheid.Kracht), new("N",1.0) },
                { (EenheidSysteem.NewtonMillimeter, Grootheid.Moment), new("Nmm",  1.0) },
                { (EenheidSysteem.NewtonMillimeter, Grootheid.Oppervlak), new("mm²",  1.0) },
                { (EenheidSysteem.NewtonMillimeter, Grootheid.Volume), new("mm³",  1.0) },
                { (EenheidSysteem.NewtonMillimeter, Grootheid.Weerstandsmoment), new("mm³",  1.0) },
                { (EenheidSysteem.NewtonMillimeter, Grootheid.Traagheid), new("mm⁴",  1.0) },
                { (EenheidSysteem.NewtonMillimeter, Grootheid.Spanning), new("N/mm²",1.0) },
                { (EenheidSysteem.NewtonMillimeter, Grootheid.Rek), new("-",    1.0) },
                { (EenheidSysteem.NewtonMillimeter, Grootheid.Veerwaarde), new("N/mm", 1.0) },
                { (EenheidSysteem.NewtonMillimeter, Grootheid.Stijfheid), new("Nmm²", 1.0)},
                { (EenheidSysteem.NewtonMillimeter, Grootheid.Kromming), new("mm⁻¹", 1.0)},


            // --- SI (kN, m) ---
                { (EenheidSysteem.KiloNewtonMeter, Grootheid.Lengte), new("m", 1.0e3) },
                { (EenheidSysteem.KiloNewtonMeter, Grootheid.Kracht), new("kN",1.0e3) },
                { (EenheidSysteem.KiloNewtonMeter, Grootheid.Moment), new("kNm",1.0e6) },
                { (EenheidSysteem.KiloNewtonMeter, Grootheid.Oppervlak), new("m²",1.0e6) },
                { (EenheidSysteem.KiloNewtonMeter, Grootheid.Volume), new("m³", 1.0e9) },
                { (EenheidSysteem.KiloNewtonMeter, Grootheid.Weerstandsmoment), new("m³", 1.0e9) },
                { (EenheidSysteem.KiloNewtonMeter, Grootheid.Traagheid), new("m⁴",   1.0e12) },
                { (EenheidSysteem.KiloNewtonMeter, Grootheid.Spanning), new("kN/m²",1.0e-3) },
                { (EenheidSysteem.KiloNewtonMeter, Grootheid.Rek), new("-",   1.0) },
                { (EenheidSysteem.KiloNewtonMeter, Grootheid.Veerwaarde), new("kN/m",1.0) },
                { (EenheidSysteem.KiloNewtonMeter, Grootheid.Stijfheid), new("kNm²", 1.0e9) },
                { (EenheidSysteem.KiloNewtonMeter, Grootheid.Kromming), new("m⁻¹", 1.0e-3) },




                // --- US Imperial (inch, lb) ---
                { (EenheidSysteem.USImperial, Grootheid.Lengte), new("in", 25.4) },                   // 1 in = 25.4 mm
                { (EenheidSysteem.USImperial, Grootheid.Kracht), new("lbf", 4.4482216152605) },       // 1 lbf ≈ 4.448 N
                { (EenheidSysteem.USImperial, Grootheid.Moment), new("lbf·in", 113.0) },              // 1 lbf·in ≈ 113 Nmm
                { (EenheidSysteem.USImperial, Grootheid.Oppervlak), new("in²", 645.16) },                // 1 in² = 645.16 mm²
                { (EenheidSysteem.USImperial, Grootheid.Volume), new("in³", 16_387.0) },              // 1 in³ = 16387 mm³
                { (EenheidSysteem.USImperial, Grootheid.Traagheid), new("in⁴", 41_623.0) },              // 1 in⁴ = 41623 mm⁴
                { (EenheidSysteem.USImperial, Grootheid.Spanning), new("psi", 0.00689476) },            // 1 psi = 0.00689476 N/mm²
                { (EenheidSysteem.USImperial, Grootheid.Rek), new("-", 1.0) },
                { (EenheidSysteem.USImperial, Grootheid.Veerwaarde), new("lbf/in", 0.1751) },              // 1 lbf/in ≈ 0.1751 N/mm
                { (EenheidSysteem.USImperial, Grootheid.Stijfheid), new("lbf·in²", 4.44822 * 645.16)},                   // ≈ 2868 N·mm²
                { (EenheidSysteem.USImperial, Grootheid.Weerstandsmoment), new("in³", 25.4 * 25.4 * 25.4) },               // 16,387 mm³
                { (EenheidSysteem.USImperial, Grootheid.Kromming), new("1/in", 1.0 / 25.4) }                                 // ≈ 0.03937 1/mm

            };

        public static UnitDefinition GetUnit(EenheidSysteem systeem, Grootheid grootheid)
            => _units[(systeem, grootheid)];

        public static string GetUnitSymbol(EenheidSysteem systeem, Grootheid grootheid)
            => GetUnit(systeem, grootheid).Symbool;

        public static double GetFactor(EenheidSysteem systeem, Grootheid grootheid)
            => GetUnit(systeem, grootheid).FactorToBase;

        // van geselecteerde eenheid naar basis (N, mm)
        public static double ToBase(double value, EenheidSysteem systeem, Grootheid grootheid)
        {
            var def = GetUnit(systeem, grootheid);
            return value * def.FactorToBase;
        }

        // van basis naar geselecteerde eenheid
        public static double FromBase(double baseValue, EenheidSysteem systeem, Grootheid grootheid)
        {
            var def = GetUnit(systeem, grootheid);
            return baseValue / def.FactorToBase;
        }
    }


    public class EenhedenGroothedenDemoModel
    {
        // Waarden worden altijd in basis opgeslagen (N, mm)
        public Dictionary<Grootheid, double> BasisWaarden { get; set; } = new();

        public EenhedenGroothedenDemoModel()
        {
            foreach (var grootheid in Enum.GetValues<Grootheid>())
            {
                BasisWaarden[grootheid] = 1;
            }
        }

        public double GetValue(EenheidSysteem systeem, Grootheid grootheid)
        {
            var baseValue = BasisWaarden[grootheid];
            return UnitHelper.FromBase(baseValue, systeem, grootheid);
        }

        public void SetValue(EenheidSysteem systeem, Grootheid grootheid, double value)
        {
            BasisWaarden[grootheid] = UnitHelper.ToBase(value, systeem, grootheid);
        }
    }




}
