using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eurocode.BetonConstructies.StrutAndTie
{
    public enum StrutAndTieNodeType
    {
        CCC,
        CCT,
        CTT
    }

    public enum StrutAndTieMemberType
    {
        Compression,
        Tension
    }

    public enum NodeFaceType
    {
        Vertical,
        Horizontal,
        Diagonal,
        Custom
    }

    public readonly record struct Vector2D(double X, double Y)
    {
        public double Length => Math.Sqrt(X * X + Y * Y);

        public Vector2D Normalize()
        {
            var length = Length;

            if (length <= 0.0)
                throw new InvalidOperationException(
                    "Een vector met lengte nul kan niet worden genormaliseerd.");

            return new Vector2D(X / length, Y / length);
        }

        public static Vector2D FromAngle(double angleRad)
            => new(Math.Cos(angleRad), Math.Sin(angleRad));
    }

    public sealed class StrutAndTieForce
    {
        public required string Id { get; init; }

        public required string Name { get; init; }

        public required StrutAndTieMemberType MemberType { get; init; }

        /// <summary>
        /// Positieve grootte van de kracht in N.
        /// </summary>
        public required double Magnitude { get; init; }

        /// <summary>
        /// Richting waarin de kracht op de knoop werkt.
        /// </summary>
        public required Vector2D Direction { get; init; }

        public Vector2D UnitDirection => Direction.Normalize();

        public double Fx => Magnitude * UnitDirection.X;

        public double Fy => Magnitude * UnitDirection.Y;

        public double AngleRad => Math.Atan2(UnitDirection.Y, UnitDirection.X);

        public double AngleDeg => AngleRad * 180.0 / Math.PI;
    }

    public sealed class StrutAndTieNodeFace
    {
        public required string Id { get; init; }

        public required string Name { get; init; }

        public required NodeFaceType FaceType { get; init; }

        /// <summary>
        /// Kracht die normaal op het knoopvlak werkt.
        /// </summary>
        public required StrutAndTieForce NormalForce { get; init; }

        /// <summary>
        /// Lengte van het knoopvlak in mm.
        /// Bijvoorbeeld x1, y1 of w.
        /// </summary>
        public required double Length { get; init; }

        /// <summary>
        /// Constructiebreedte loodrecht op het 2D STM-model, in mm.
        /// Meestal de breedte b van de console.
        /// </summary>
        public required double Thickness { get; init; }

        /// <summary>
        /// Toelaatbare knoopspanning in N/mm².
        /// </summary>
        public required double SigmaRdMax { get; init; }

        public double Area => Length * Thickness;

        public double SigmaEd =>
            Area > 0.0
                ? NormalForce.Magnitude / Area
                : double.PositiveInfinity;

        public double UnityCheck =>
            SigmaRdMax > 0.0
                ? SigmaEd / SigmaRdMax
                : double.PositiveInfinity;

        public bool IsValid =>
            Length > 0.0 &&
            Thickness > 0.0 &&
            SigmaRdMax > 0.0;

        public bool IsOk => IsValid && SigmaEd <= SigmaRdMax;
    }

    public readonly record struct Point2D(double X, double Y);


    public sealed class StrutAndTieNodeGeometry
    {
        /// <summary>
        /// Rekenkundig knooppunt, bijvoorbeeld het snijpunt van staafassen.
        /// </summary>
        public Point2D Center { get; init; }

        /// <summary>
        /// Optionele contourpunten voor SVG-weergave.
        /// </summary>
        public IReadOnlyList<Point2D> Polygon { get; init; } = [];
    }

    public sealed class StrutAndTieNode
    {
        private readonly List<StrutAndTieForce> _forces = [];
        private readonly List<StrutAndTieNodeFace> _faces = [];

        public required string Id { get; init; }

        public required string Name { get; init; }

        public required StrutAndTieNodeType Type { get; init; }

        public StrutAndTieNodeGeometry Geometry { get; init; } = new();

        public IReadOnlyList<StrutAndTieForce> Forces => _forces;

        public IReadOnlyList<StrutAndTieNodeFace> Faces => _faces;

        public double SumFx => _forces.Sum(x => x.Fx);

        public double SumFy => _forces.Sum(x => x.Fy);

        public double ForceImbalance =>
            Math.Sqrt(SumFx * SumFx + SumFy * SumFy);

        public double TotalForce =>
            _forces.Sum(x => x.Magnitude);

        /// <summary>
        /// Relatieve afwijking van het krachtenevenwicht.
        /// </summary>
        public double RelativeForceImbalance =>
            TotalForce > 0.0
                ? ForceImbalance / TotalForce
                : 0.0;

        public bool IsInEquilibrium =>
            RelativeForceImbalance <= 1e-6;

        public bool AreFacesOk =>
            _faces.Count > 0 &&
            _faces.All(x => x.IsOk);

        public bool IsOk =>
            IsInEquilibrium &&
            AreFacesOk;

        public void AddForce(StrutAndTieForce force)
        {
            ArgumentNullException.ThrowIfNull(force);

            if (_forces.Any(x => x.Id == force.Id))
                throw new InvalidOperationException(
                    $"Er bestaat al een kracht met Id '{force.Id}'.");

            if (force.Magnitude < 0.0)
                throw new ArgumentOutOfRangeException(
                    nameof(force),
                    "De krachtgrootte moet positief zijn. Gebruik Direction voor het teken.");

            _forces.Add(force);
        }

        public void AddFace(StrutAndTieNodeFace face)
        {
            ArgumentNullException.ThrowIfNull(face);

            if (_faces.Any(x => x.Id == face.Id))
                throw new InvalidOperationException(
                    $"Er bestaat al een knoopvlak met Id '{face.Id}'.");

            if (!_forces.Contains(face.NormalForce))
                throw new InvalidOperationException(
                    $"De kracht '{face.NormalForce.Id}' is nog niet aan de knoop toegevoegd.");

            _faces.Add(face);
        }


    }

    public static class StrutAndTieNodeFaceFactory
    {
        public static StrutAndTieNodeFace CreateFullyUtilized(
            string id,
            string name,
            NodeFaceType faceType,
            StrutAndTieForce normalForce,
            double thickness,
            double sigmaRdMax)
        {
            ArgumentNullException.ThrowIfNull(normalForce);

            if (thickness <= 0.0)
                throw new ArgumentOutOfRangeException(nameof(thickness));

            if (sigmaRdMax <= 0.0)
                throw new ArgumentOutOfRangeException(nameof(sigmaRdMax));

            var length =
                normalForce.Magnitude /
                (thickness * sigmaRdMax);

            return new StrutAndTieNodeFace
            {
                Id = id,
                Name = name,
                FaceType = faceType,
                NormalForce = normalForce,
                Length = length,
                Thickness = thickness,
                SigmaRdMax = sigmaRdMax
            };
        }
    }















}