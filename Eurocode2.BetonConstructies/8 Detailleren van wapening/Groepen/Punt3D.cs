namespace Eurocode.BetonConstructies
{
    /// <summary>
    /// Eenvoudig 3D-punt [mm] voor wapeninggeometrie — bewust zonder
    /// THREE-/Tekla-afhankelijkheid, serialiseerbaar naar JSON.
    /// </summary>
    public readonly record struct Punt3D(double X, double Y, double Z)
    {
        public static Punt3D operator +(Punt3D a, Punt3D b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Punt3D operator -(Punt3D a, Punt3D b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Punt3D operator *(double f, Punt3D p) => new(f * p.X, f * p.Y, f * p.Z);

        public double Lengte => Math.Sqrt(X * X + Y * Y + Z * Z);

        public static double Afstand(Punt3D a, Punt3D b) => (b - a).Lengte;

        /// <summary> Lineaire interpolatie tussen twee punten (t = 0..1). </summary>
        public static Punt3D Lerp(Punt3D a, Punt3D b, double t) => a + t * (b - a);

        public static double Dot(Punt3D a, Punt3D b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

        public static Punt3D Cross(Punt3D a, Punt3D b) => new(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X);

        public Punt3D Genormaliseerd()
        {
            var l = Lengte;
            return l > 0 ? new Punt3D(X / l, Y / l, Z / l) : this;
        }
    }
}
