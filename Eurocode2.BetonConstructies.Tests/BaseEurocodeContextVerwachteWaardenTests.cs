using CommonLibrary;
using Profielen.Parametrisch;
using Eurocode.BetonConstructies;
using System.Reflection;

namespace Eurocode2.BetonConstructies.Tests
{

    public class DoorbuigingStudieTests
    {
        [Theory]
        [InlineData(10.0, 40.0, 500.0, 700.0, 43.822061)]

        public void TestDoorbuigingStudie(
            double L,
            double lijnlast,
            double b,
            double h,
            double expectedDoorbuiging)
        {
            // Arrange
            var studie = new DoorbuigingStudie(L, lijnlast)
            {
                Optrede = 0, // geen optrede zodat het de lengte niet wordt beïnvloed
                Profiel = new ParametrischProfielContext { Breedte = b, Hoogte = h }
            };

            // Act
            var resultaat = studie.DoorbuigingEenvoudig;

            // Assert
            Assert.Equal(expectedDoorbuiging, resultaat, 3); // afronden op 2 decimalen
        }





    }


    public class BaseEurocodeContextVerwachteWaardenTests
    {
        // Verwachte waarden per context
        public static readonly Dictionary<string, (string PropertyName, double ExpectedValue, int Precision)> VerwachteWaarden =
            new()
            {
                { nameof(BetonContext) + ".Fcd", ("Fcd", 20.0, 6) },
                { nameof(BetonContext) + ".FctkVijfProcent", ("FctkVijfProcent", 2.0275277, 6) },
                { nameof(BetonContext) + ".Fctd", ("Fctd", 1.3516851, 6) },
                { nameof(BetonStaalContext) + ".Fyd", ("Fyd", 500/1.15, 1) },

            };

        public static IEnumerable<object[]> TestData()
        {
            foreach (var item in VerwachteWaarden)
            {
                var parts = item.Key.Split('.');
                var typeName = parts[0];
                var propertyName = item.Value.PropertyName;
                yield return new object[] { typeName, propertyName, item.Value.ExpectedValue, item.Value.Precision };
            }
        }

        private static Assembly[] GetEurocodeAssemblies()
        {
            // Alle geladen assemblies waarvan de naam "Eurocode" bevat
            var loaded = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && a.GetName().Name != null &&
                            a.GetName().Name.StartsWith("Eurocode", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Zorg dat minstens de assembly met BaseEurocodeContext aanwezig is
            var baseAsm = typeof(BaseEurocodeContext).Assembly;
            if (!loaded.Contains(baseAsm))
                loaded.Add(baseAsm);

            return loaded.Distinct().ToArray();
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void Eigenschappen_HebbenVerwachteWaarden(string typeName, string propertyName, double expected, int precision)
        {
            var assemblies = GetEurocodeAssemblies();
            Assert.True(assemblies.Any(), "Geen assemblies gevonden om te scannen. Controleer project-references.");

            // Zoek type in alle assemblies
            var type = assemblies
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name == typeName);

            Assert.NotNull(type); // duidelijke fout als type niet gevonden wordt
            var concreteType = type!;

            // Probeer instance te maken
            object? rawInstance = null;
            try
            {
                rawInstance = Activator.CreateInstance(concreteType);
            }
            catch (MissingMethodException)
            {
                Assert.Fail($"Type '{concreteType.FullName}' heeft geen parameterloze constructor.");
            }
            catch (TargetInvocationException tie)
            {
                Assert.Fail($"Constructor van '{concreteType.FullName}' gooit exception: {tie.InnerException?.Message}");
            }

            Assert.NotNull(rawInstance);

            // ✅ Vervang Assert.IsType door IsAssignableFrom
            Assert.IsAssignableFrom<BaseEurocodeContext>(rawInstance);
            var instance = (BaseEurocodeContext)rawInstance!;

            // Stel concrete defaults in indien nodig
            if (instance is BetonContext beton)
            {
                beton.Betonsterkteklasse = BetonsterkteklasseEnum.C30_37;
            }

            // Property ophalen en checken
            var prop = concreteType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(prop);

            var valueObj = prop.GetValue(instance);
            Assert.NotNull(valueObj);

            double actual;
            try
            {
                actual = Convert.ToDouble(valueObj);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Kon waarde van property '{propertyName}' op type '{concreteType.FullName}' niet naar double converteren: {ex.Message}");
                return; // unreachable
            }

            Assert.Equal(expected, actual, precision: precision);
        }
    }
}
