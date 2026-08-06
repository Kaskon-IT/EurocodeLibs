namespace Eurocode.BetonConstructies
{
    public static class GedrongenUitkragingCalculator
    {
        public static GedrongenUitkragingResult Calculate(GedrongenUitkragingInput input)
        {
            var result = new GedrongenUitkragingResult();
            // Bereken z volgens de formule: z = 0.4 * a + 0.4 h <= 1.6 * a
            // waarbij a gelijk is aan ac (afstand van F tot kolomrand) + ar (afstand van Reactie tot kolomrand)


            List<double> helpers = [input.Ab / 2.0, input.L / 4.0, input.H / 4.0];
            double ar = helpers.Min();


            double a = input.Ac + ar;
            double h = input.H;
            double z = Math.Min(0.4 * a + 0.4 * h, 1.6 * a);
            result.Z = z;
            result.A = a;
            result.H = h;
            return result;
        }
    }
}
