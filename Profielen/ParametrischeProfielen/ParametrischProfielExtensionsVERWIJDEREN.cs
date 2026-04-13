using static Kaskon_it.Algemeen.Geometrie;

namespace Profielen.ParametrischBAK
{
    public static class ParametrischProfielExtensionsVERWIJDEREN
    {
        public static PuntD[] GetPolygon(this ParametrischProfielContextVERWIJDEREN context)
        {
            List<PuntD> polygon = [];

            double x = 0, y = 0, z = 0;
            switch (context.Vorm)
            {
                default:
                case ParametrischeProfielVormEnumVERWIJDEREN.Rechthoek:
                    polygon.Add(new(x, y, z));
                    y += context.Hoogte; polygon.Add(new(x, y, z));
                    x += context.Breedte; polygon.Add(new PuntD(x, y, z));
                    y -= context.Hoogte; polygon.Add(new PuntD(x, y, z));
                    polygon.Add(new());
                    break;
                case ParametrischeProfielVormEnumVERWIJDEREN.L1:
                    polygon.Add(new(x, y, z));
                    y += context.Hoogte; polygon.Add(new(x, y, z));
                    x += context.Breedte - context.B1; polygon.Add(new(x, y, z));
                    y -= context.H1; polygon.Add(new(x, y, z));
                    x += context.B1; polygon.Add(new(x, y, z));
                    y -= (context.Hoogte - context.H1); polygon.Add(new(x, y, z));
                    polygon.Add(new());
                    break;
                case ParametrischeProfielVormEnumVERWIJDEREN.L2:
                    polygon.Add(new());
                    y += (context.Hoogte - context.H1);
                    polygon.Add(new(x, y, z));
                    x += context.B1;
                    polygon.Add(new(x, y, z));
                    y += context.H1;
                    polygon.Add(new(x, y, z));
                    x += (context.Breedte - context.B1);
                    polygon.Add(new(x, y, z));
                    y = 0;
                    polygon.Add(new(x, y, z));
                    x = 0;
                    polygon.Add(new(x, y, z));
                    break;
                case ParametrischeProfielVormEnumVERWIJDEREN.L3:
                    polygon.Add(new());
                    y = context.Hoogte;
                    polygon.Add(new(x, y, z));
                    x = context.Breedte;
                    polygon.Add(new(x, y, z));
                    y -= (context.Hoogte - context.H1);
                    polygon.Add(new(x, y, z));
                    x -= context.B1;
                    polygon.Add(new(x, y, z));
                    y = 0;
                    polygon.Add(new(x, y, z));
                    x = 0;
                    polygon.Add(new(x, y, z));
                    break;
                case ParametrischeProfielVormEnumVERWIJDEREN.L4:
                    y = context.H1;
                    polygon.Add(new(x, y, z));
                    y = context.Hoogte;
                    polygon.Add(new(x, y, z));
                    x = context.Breedte;
                    polygon.Add(new(x, y, z));
                    y = 0;
                    polygon.Add(new(x, y, z));
                    x = context.B1;
                    polygon.Add(new(x, y, z));
                    y = context.H1;
                    polygon.Add(new(x, y, z));
                    x = 0;
                    polygon.Add(new(x, y, z));
                    break;
                case ParametrischeProfielVormEnumVERWIJDEREN.T1:
                    y = context.Hoogte;
                    polygon.Add(new(x, y, z));
                    x = context.Breedte;
                    polygon.Add(new(x, y, z));
                    y = context.H2;
                    polygon.Add(new(x, y, z));
                    x = context.Breedte - context.B2;
                    polygon.Add(new(x, y, z));
                    y = 0;
                    polygon.Add(new(x, y, z));
                    x = context.B1;
                    polygon.Add(new(x, y, z));
                    y = context.H1;
                    polygon.Add(new(x, y, z));
                    x = 0;
                    polygon.Add(new(x, y, z));
                    y = context.Hoogte;
                    polygon.Add(new(x, y, z));
                    break;
                case ParametrischeProfielVormEnumVERWIJDEREN.T2:
                    y = 0;
                    polygon.Add(new(x, y, z));
                    y = context.Hoogte - context.H1;
                    polygon.Add(new(x, y, z));
                    x = context.B1;
                    polygon.Add(new(x, y, z));
                    y = context.Hoogte;
                    polygon.Add(new(x, y, z));
                    x = context.Breedte - context.B2;
                    polygon.Add(new(x, y, z));
                    y -= context.H2;
                    polygon.Add(new(x, y, z));
                    x = context.Breedte;
                    polygon.Add(new(x, y, z));
                    y = 0;
                    polygon.Add(new(x, y, z));
                    x = 0;
                    polygon.Add(new(x, y, z));
                    break;
                case ParametrischeProfielVormEnumVERWIJDEREN.T3:
                    y = 0;
                    polygon.Add(new(x, y, z));
                    y = context.Hoogte;
                    polygon.Add(new(x, y, z));
                    x = context.Breedte - context.B1;
                    polygon.Add(new(x, y, z));
                    y -= context.H1;
                    polygon.Add(new(x, y, z));

                    x = context.Breedte;
                    polygon.Add(new(x, y, z));

                    y -= (context.Hoogte - context.H1 - context.H2);
                    polygon.Add(new(x, y, z));

                    x -= context.B2;
                    polygon.Add(new(x, y, z));

                    y = 0;
                    polygon.Add(new(x, y, z));

                    x = 0;
                    polygon.Add(new(x, y, z));





                    break;
                case ParametrischeProfielVormEnumVERWIJDEREN.T4:
                    y = context.H1;
                    polygon.Add(new(x, y, z));

                    y = context.Hoogte - context.H2;
                    polygon.Add(new(x, y, z));

                    x += context.B2;
                    polygon.Add(new(x, y, z));

                    y = context.Hoogte;
                    polygon.Add(new(x, y, z));

                    x = context.Breedte;
                    polygon.Add(new(x, y, z));

                    y = 0;
                    polygon.Add(new(x, y, z));

                    x = context.B1;
                    polygon.Add(new(x, y, z));

                    y += context.H1;
                    polygon.Add(new(x, y, z));

                    x = 0;
                    polygon.Add(new(x, y, z));







                    break;


                case ParametrischeProfielVormEnumVERWIJDEREN.U1:
                    polygon.Add(new(x, y, z));
                    y = context.Hoogte;
                    polygon.Add(new(x, y, z));
                    x = (context.Breedte - context.B1) / 2;
                    polygon.Add(new(x, y, z));
                    y -= context.H1;
                    polygon.Add(new(x, y, z));
                    x += context.B1;
                    polygon.Add(new(x, y, z));
                    y = context.Hoogte;
                    polygon.Add(new(x, y, z));
                    x = context.Breedte;
                    polygon.Add(new(x, y, z));
                    y = 0;
                    polygon.Add(new(x, y, z));
                    x = 0;
                    polygon.Add(new(x, y, z));
                    break;
                case ParametrischeProfielVormEnumVERWIJDEREN.U2:
                    polygon.Add(new(x, y, z));
                    y = context.Hoogte;
                    polygon.Add(new(x, y, z));
                    x = context.Breedte;
                    polygon.Add(new(x, y, z));
                    y = 0;
                    polygon.Add(new(x, y, z));
                    x -= ((context.Breedte - context.B1) / 2);
                    polygon.Add(new(x, y, z));
                    y += context.H1;
                    polygon.Add(new(x, y, z));
                    x -= context.B1;
                    polygon.Add(new(x, y, z));
                    y = 0;
                    polygon.Add(new(x, y, z));
                    x = 0;
                    polygon.Add(new(x, y, z));
                    break;

                case ParametrischeProfielVormEnumVERWIJDEREN.I:
                    y = 0;
                    polygon.Add(new(x, y, z));
                    y = (context.Hoogte - context.H1) / 2;
                    polygon.Add(new(x, y, z));
                    x = context.B1;
                    polygon.Add(new(x, y, z));

                    y += context.H1;
                    polygon.Add(new(x, y, z));

                    x = 0;
                    polygon.Add(new(x, y, z));

                    y = context.Hoogte;
                    polygon.Add(new(x, y, z));

                    x = context.Breedte;
                    polygon.Add(new(x, y, z));

                    y -= ((context.Hoogte - context.H2) / 2);
                    polygon.Add(new(x, y, z));

                    x -= context.B2;
                    polygon.Add(new(x, y, z));

                    y -= context.H2;
                    polygon.Add(new(x, y, z));

                    x = context.Breedte;
                    polygon.Add(new(x, y, z));

                    y = 0;
                    polygon.Add(new(x, y, z));

                    x = 0;
                    polygon.Add(new(x, y, z));


                    break;




            }
            return [.. polygon];

        }

        public static PuntD[] GetPolygonRondomZwaartePunt(this ParametrischProfielContextVERWIJDEREN context, PuntD zwaartepunt)
        {
            //PuntD puntD = ZoekZwaartePunt(polygon);
            PuntD[] array = new PuntD[context.Polygon.Length];
            context.Polygon.CopyTo(array, 0);
            for (int i = 0; i < context.Polygon.Length; i++)
            {
                array[i].X -= zwaartepunt.X;
                array[i].Y -= zwaartepunt.Y;
            }

            return array;
        }

    }


}
