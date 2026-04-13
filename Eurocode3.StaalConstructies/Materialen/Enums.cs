using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eurocode.StaalConstructies
{
    /// <summary>
    /// Constructiestaalkwaliteiten volgens Eurocode 3 (EN 10025-2)
    /// Zonder lage-temperatuur aanduiding (JR/J0/J2)
    /// </summary>
    public enum StaalKwaliteitEnum
    {
        S235,
        S275,
        S355,
        S450,
        S500
    }

    public static class StaalKwaliteitEnumExtensions
    {
        public static double GetFy(this StaalKwaliteitEnum? staalKwaliteit)
        {
            return (staalKwaliteit ?? StaalKwaliteitEnum.S235) switch
            {
                StaalKwaliteitEnum.S235 => 235,
                StaalKwaliteitEnum.S275 => 275,
                StaalKwaliteitEnum.S355 => 355,
                StaalKwaliteitEnum.S450 => 450,
                StaalKwaliteitEnum.S500 => 500,
                _ => throw new ArgumentOutOfRangeException(nameof(staalKwaliteit), "Onbekende staalkwaliteit"),
            };
        }
        public static double GetFu(this StaalKwaliteitEnum? staalKwaliteit)
        {
            return (staalKwaliteit ?? StaalKwaliteitEnum.S235) switch
            {
                StaalKwaliteitEnum.S235 => 360,
                StaalKwaliteitEnum.S275 => 430,
                StaalKwaliteitEnum.S355 => 510,
                StaalKwaliteitEnum.S450 => 570,
                StaalKwaliteitEnum.S500 => 620,
                _ => throw new ArgumentOutOfRangeException(nameof(staalKwaliteit), "Onbekende staalkwaliteit"),
            };
        }
    }
}
