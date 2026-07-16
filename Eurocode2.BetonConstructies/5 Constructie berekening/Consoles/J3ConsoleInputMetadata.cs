using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLibrary.Models.Metadata;

namespace Eurocode.BetonConstructies.Consoles
{
    public static class J3ConsoleInputMetadata
    {
        public static IModelMetadata<J3ConsoleInput> Create()
        {
            Dictionary<string, PropertyMetadata<J3ConsoleInput>> properties = new()
            {
                [nameof(J3ConsoleInput.FEd)] =
                    new NumericPropertyMetadata<J3ConsoleInput>
                    {
                        PropertyName = nameof(J3ConsoleInput.FEd),

                        Label = "<i>F</i><sub>Ed</sub>",
                        Description = "Rekenwaarde van de verticale belasting.",
                        Unit = "kN",
                        Order = 10,

                        Min = _ => 0,
                        Max = _ => 5_000,
                        Step = _ => 10,

                        Decimals = 0
                    },

                [nameof(J3ConsoleInput.HEd)] =
                    new NumericPropertyMetadata<J3ConsoleInput>
                    {
                        PropertyName = nameof(J3ConsoleInput.HEd),

                        Label = "<i>H</i><sub>Ed</sub>",
                        Description =
                            "De horizontale belasting mag niet groter zijn dan FEd.",
                        Unit = "kN",
                        Order = 20,

                        Min = _ => 0,

                        // Dynamische maximumwaarde
                        Max = model => model.FEd,

                        Step = _ => 10,

                        Decimals = 0
                    },

                [nameof(J3ConsoleInput.Lc)] =
                    new NumericPropertyMetadata<J3ConsoleInput>
                    {
                        PropertyName = nameof(J3ConsoleInput.Lc),

                        Label = "Lengte console",
                        Unit = "mm",
                        Order = 30,

                        Min = _ => 50,
                        Max = _ => 2_000,
                        Step = _ => 10,

                        Decimals = 0
                    }
            };

            return new ModelMetadata<J3ConsoleInput>(properties);
        }
    }
}
