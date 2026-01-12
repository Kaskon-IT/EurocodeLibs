using CommonLibrary.Interfaces;

namespace CommonLibrary.Materialen;

public sealed class DummyMateriaal : IMateriaal
{
    public static readonly DummyMateriaal Instance = new();

    private DummyMateriaal() { }

    public string Naam { get; set; } = "Geen Materiaal";
  

    public string Eurocode => "NVT";

    public double Dichtheid => 1e-3;
    public double SoortelijkGewicht => 1.0; // 1 kg/m³
    public double E => 1;

    public double GammaM => 1.5;

    public double GammaM0 => 1.0;

    public double GammaM1 => 1.0;

    public double GammaM2 => 1.25;
}
