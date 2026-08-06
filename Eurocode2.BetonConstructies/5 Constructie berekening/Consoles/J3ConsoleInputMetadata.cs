using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection.Emit;
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
                        Min = _ => 1,
                        Max = _ => 5_000,
                        Step = _ => 50,
                        Decimals = 0
                    },

                [nameof(J3ConsoleInput.HEd)] =
                    new NumericPropertyMetadata<J3ConsoleInput>
                    {
                        PropertyName = nameof(J3ConsoleInput.HEd),
                        Label = "<i>H</i><sub>Ed</sub>",
                        Description = "Rekenwaarde van de horizontale belasting.",
                        Unit = "kN",
                        Order = 20,
                        Min = _ => 0,
                        Max = model => model.FEd, // Dynamische maximumwaarde
                        Step = _ => 10,
                        Decimals = 0
                    },

                [nameof(J3ConsoleInput.Lc)] =
                    new NumericPropertyMetadata<J3ConsoleInput>
                    {
                        PropertyName = nameof(J3ConsoleInput.Lc),
                        Label = "<i>L</i><sub>c</sub>",
                        Unit = "mm",
                        Order = 30,
                        Min = model => Math.Max(model.Ac + model.LoadPlateLength / 2.0 + model.Dekking, 100),
                        Max = _ => 2_000,
                        Step = _ => 10,
                        Decimals = 0
                    },
                [nameof(J3ConsoleInput.Ac)] =
                    new NumericPropertyMetadata<J3ConsoleInput>
                    {
                        PropertyName = nameof(J3ConsoleInput.Ac),
                        Label = "<i>a</i><sub>c</sub>",
                        Description = "Afstand van de rand van de console tot de belasting.",
                        Unit = "mm",
                        Order = 40,
                        Min = _ => 50,
                        Max = _ => 500,
                        Step = _ => 5,
                        Decimals = 0
                    },
                [nameof(J3ConsoleInput.LoadPlateLength)] =
                    new NumericPropertyMetadata<J3ConsoleInput>
                    {
                        PropertyName = nameof(J3ConsoleInput.LoadPlateLength),
                        Label = "<i>L</i><sub>plate</sub>",
                        Description = "Lengte van de oplegplaat.",
                        Unit = "mm",
                        Order = 50,
                        Min = _ => 50,
                        Max = model => model.Lc - model.Dekking,
                        Step = _ => 5,
                        Decimals = 0
                    },
                [nameof(J3ConsoleInput.LoadPlateWidth)] =
                    new NumericPropertyMetadata<J3ConsoleInput>
                    {
                        PropertyName = nameof(J3ConsoleInput.LoadPlateWidth),
                        Label = "<i>B</i><sub>plate</sub>",
                        Description = "Breedte van de oplegplaat.",
                        Unit = "mm",
                        Order = 60,
                        Min = _ => 50,
                        Max = model => model.Bc,
                        Step = _ => 5,
                        Decimals = 0
                    },
                [nameof(J3ConsoleInput.DikteOplegmateriaal)] =
                    new NumericPropertyMetadata<J3ConsoleInput>
                    {
                        PropertyName = nameof(J3ConsoleInput.DikteOplegmateriaal),
                        Label = "<i>d</i><sub>plate</sub>",
                        Description = "Dikte van het oplegmateriaal.",
                        Unit = "mm",
                        Order = 65,
                        Min = _ => 0,
                        Max = _ => 50,
                        Step = _ => 5,
                        Decimals = 0
                    },
                [nameof(J3ConsoleInput.Bc)] =
                    new NumericPropertyMetadata<J3ConsoleInput>
                    {
                        PropertyName = nameof(J3ConsoleInput.Bc),
                        Label = "<i>B</i><sub>c</sub>",
                        Description = "Breedte van de console.",
                        Unit = "mm",
                        Order = 70,
                        Min = _ => 100,
                        Max = _ => 1_000,
                        Step = _ => 10,
                        Decimals = 0
                    },
                [nameof(J3ConsoleInput.Hc)] =
                    new NumericPropertyMetadata<J3ConsoleInput>
                    {
                        PropertyName = nameof(J3ConsoleInput.Hc),
                        Label = "<i>H</i><sub>c</sub>",
                        Description = "Hoogte van de console.",
                        Unit = "mm",
                        Order = 75,
                        Min = _ => 100,
                        Max = _ => 1_000,
                        Step = _ => 10,
                        Decimals = 0
                    },
                [nameof(J3ConsoleInput.Dekking)] =
                    new NumericPropertyMetadata<J3ConsoleInput>
                    {
                        PropertyName = nameof(J3ConsoleInput.Dekking),
                        Label = "<i>c</i><sub>prov</sub>",
                        Description = "Dekking van de wapening.",
                        Unit = "mm",
                        Order = 80,
                        Min = _ => 15,
                        Max = _ => 100,
                        Step = _ => 1,
                        Decimals = 0
                    },
                [nameof(J3ConsoleInput.HoofdstaafDiameter)] =
                    new NumericPropertyMetadata<J3ConsoleInput>
                    {
                        PropertyName = nameof(J3ConsoleInput.HoofdstaafDiameter),
                        Label = "<i>Ø</i><sub>hoofd</sub>",
                        Description = "Diameter van de hoofdstaaf.",
                        Unit = "mm",
                        Order = 90,
                        Min = _ => 8,
                        Max = _ => 32,
                        Step = _ => 1,
                        Decimals = 0
                    },
                [nameof(J3ConsoleInput.HoofdstaafAantal)] =
                    new NumericPropertyMetadata<J3ConsoleInput>
                    {
                        PropertyName = nameof(J3ConsoleInput.HoofdstaafAantal),
                        Label = "<i>n</i><sub>hoofd</sub>",
                        Description = "Aantal hoofdstaven.",
                        Unit = "st",
                        Order = 100,
                        Min = _ => 2,
                        Max = _ => 10,
                        Step = _ => 1,
                        Decimals = 0
                    },

                [nameof(J3ConsoleInput.HoofdstaafBuigdoornDiameterFactor)] =
                    new NumericPropertyMetadata<J3ConsoleInput>
                    {
                        PropertyName = nameof(J3ConsoleInput.HoofdstaafBuigdoornDiameterFactor),
                        Label = "buigd.",
                        Description = "Factor voor buigdoorndiameter",
                        Unit = "×Ø",
                        Order = 101,
                        Min = model => model.HoofdstaafDiameter > 16 ? 7 : 4,
                        Max = _ => 1000,
                        Step = _ => 1,
                        Decimals = 0
                    },
                [nameof(J3ConsoleInput.D1Opgave)] =
                    new NumericPropertyMetadata<J3ConsoleInput>
                    {
                        PropertyName = nameof(J3ConsoleInput.D1Opgave),
                        Label = "<i>d</i><sub>1</sub>",
                        Description = "Optionele opgave van d1 (afstand hart trekband tot bovenrand); 0 = automatisch.",
                        Unit = "mm",
                        Order = 85,
                        Min = _ => 0,
                        Max = model => model.Hc / 2.0,
                        Step = _ => 5,
                        Decimals = 0
                    },
                [nameof(J3ConsoleInput.HoofdstaafDiameter2)] =
                    new NumericPropertyMetadata<J3ConsoleInput>
                    {
                        PropertyName = nameof(J3ConsoleInput.HoofdstaafDiameter2),
                        Label = "<i>Ø</i><sub>hoofd,2</sub>",
                        Description = "Diameter van de tweede laag hoofdstaven (0 = niet gebruikt).",
                        Unit = "mm",
                        Order = 102,
                        Min = _ => 0,
                        Max = _ => 32,
                        Step = _ => 1,
                        Decimals = 0
                    },
                [nameof(J3ConsoleInput.HoofdstaafAantal2)] =
                    new NumericPropertyMetadata<J3ConsoleInput>
                    {
                        PropertyName = nameof(J3ConsoleInput.HoofdstaafAantal2),
                        Label = "<i>n</i><sub>hoofd,2</sub>",
                        Description = "Aantal hoofdstaven in de tweede laag.",
                        Unit = "st",
                        Order = 103,
                        Min = _ => 0,
                        Max = _ => 10,
                        Step = _ => 1,
                        Decimals = 0
                    },
                [nameof(J3ConsoleInput.FactorBgt)] =
                    new NumericPropertyMetadata<J3ConsoleInput>
                    {
                        PropertyName = nameof(J3ConsoleInput.FactorBgt),
                        Label = "<i>f</i><sub>BGT</sub>",
                        Description = "Factor BGT/UGT voor de bruikbaarheidsgrenstoestand (scheurwijdte).",
                        Unit = "",
                        Order = 25,
                        Min = _ => 0.1,
                        Max = _ => 1.0,
                        Step = _ => 0.05,
                        Decimals = 2
                    },
                [nameof(J3ConsoleInput.ExcentriciteitBreedte)] =
                    new NumericPropertyMetadata<J3ConsoleInput>
                    {
                        PropertyName = nameof(J3ConsoleInput.ExcentriciteitBreedte),
                        Label = "<i>e</i>",
                        Description = "Excentriciteit van F<sub>Ed</sub> in de breedterichting (wringing T<sub>Ed</sub> = F<sub>Ed</sub>·e).",
                        Unit = "mm",
                        Order = 26,
                        Min = _ => 0,
                        Max = i => i.Bc / 2.0,
                        Step = _ => 5,
                        Decimals = 0
                    },
                [nameof(J3ConsoleInput.BeugelDiameter)] =
                    new NumericPropertyMetadata<J3ConsoleInput>
                    {
                        PropertyName = nameof(J3ConsoleInput.BeugelDiameter),
                        Label = "<i>Ø</i><sub>bgl</sub>",
                        Unit = "mm",
                        Order = 105,
                        Min = _ => 6,
                        Max = _ => 12,
                        Step = _ => 2,
                        Decimals = 0
                    },


                [nameof(J3ConsoleInput.Fck)] =
                    new NumericPropertyMetadata<J3ConsoleInput>
                    {
                        PropertyName = nameof(J3ConsoleInput.Fck),
                        Label = "<i>f</i><sub>ck</sub>",
                        Description = "Cilinderdruksterkte van het beton.",
                        Unit = "N/mm²",
                        Order = 110,
                        Min = _ => 20,
                        Max = _ => 90,
                        Step = _ => 5,
                        Decimals = 0
                    },
                [nameof(J3ConsoleInput.Fyk)] =
                    new NumericPropertyMetadata<J3ConsoleInput>
                    {
                        PropertyName = nameof(J3ConsoleInput.Fyk),
                        Label = "<i>f</i><sub>yk</sub>",
                        Description = "Staalkwaliteit.",
                        Unit = "N/mm²",
                        Order = 115,
                        Min = _ => 400,
                        Max = _ => 600,
                        Step = _ => 100,
                        Decimals = 0
                    },
                [nameof(J3ConsoleInput.FactorZ)] =
                    new NumericPropertyMetadata<J3ConsoleInput>
                    {
                        PropertyName = nameof(J3ConsoleInput.FactorZ),
                        Label = "<i>z/d</i>",
                        Description = "Factor voor z (bovengrens)",
                        Unit = "-",
                        Order = 115,
                        Min = _ => 0.4,
                        Max = _ => 1.0,
                        Step = _ => 0.1,
                        Decimals = 2
                    },
                [nameof(J3ConsoleInput.KolomDikte)] =
                    new NumericPropertyMetadata<J3ConsoleInput> {
                        PropertyName = nameof(J3ConsoleInput.KolomDikte),
                        Label = "<i>H<i><sub>kolom</sub>",
                        Min = _ => 200,
                        Max = _ => 1000,
                        Step = _ => 100,
                        Decimals = 0
                    },
                [nameof(J3ConsoleInput.KolomBreedte)] =
                    new NumericPropertyMetadata<J3ConsoleInput>
                    {
                        PropertyName = nameof(J3ConsoleInput.KolomBreedte),
                        Label = "<i>B<i><sub>kolom</sub>",
                        Min = model => model.Bc,
                        Max = _ => 1000,
                        Step = _ => 100,
                        Decimals = 0
                    },

                [nameof(J3ConsoleInput.DiamKolomWap)] =
                    new NumericPropertyMetadata<J3ConsoleInput>
                    {
                        PropertyName = nameof(J3ConsoleInput.DiamKolomWap),
                        Label = "<i>Ø</i><sub>kolom</sub>",
                        Min = _ =>8,
                        Max = _ => 40,
                        Step = _ => 1,
                        Decimals = 0
                    },

                [nameof(J3ConsoleInput.AfschuiningOnderzijde)] =
                    new BooleanPropertyMetadata<J3ConsoleInput>
                    {
                        PropertyName = nameof(J3ConsoleInput.AfschuiningOnderzijde),
                        Label = "Afschuining onderzijde",
                        Description = "Afschuining (verjonging) aan de onderzijde van de console. " +
                                      "Niet toegestaan als J.3(3) van toepassing is (a<sub>c</sub> > 0,5·h<sub>c</sub>, verticale beugels).",
                        Order = 75,
                        // J.3(3): bij ac > 0,5·hc zijn verticale beugels vereist en is een afschuining niet toegestaan.
                        //IsEnabled = model => model.Ac <= 0.5 * model.Hc
                    },


            };


            return new ModelMetadata<J3ConsoleInput>(properties);
        }
    }
}
