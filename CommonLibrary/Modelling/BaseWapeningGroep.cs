using System.ComponentModel;

namespace CommonLibrary.Modelling
{
    /// <summary>
    /// Gedeelde basis voor een losse wapeningsstaaf (<see cref="WapeningStaaf"/>)
    /// en een wapeningsgroep (<see cref="WapeningGroep"/>): één diameter en
    /// één of twee staafvormen.
    /// </summary>
    public abstract class BaseWapeningGroep : ModelObject, IPartOwnedModelObject
    {
        /// <summary>The physical part reinforced by this bar or reinforcement group.</summary>
        public Guid? PartId { get; set; }

        /// <summary> Staafdiameter Ø [mm]; 1 diameter per staaf/groep. </summary>
        public double Diameter { get; set; }
        public double DiameterExterior => Diameter * 1.13; // DIN488 rib is 0.065×Ø

        /// <summary>
        /// De staafvorm(en): polyline (hartlijn) + buigstralen.
        /// Standaard 1 shape (alle staven identiek). Bij een groep zijn meerdere
        /// shapes toegestaan als 'keyframes' langs de verdeellijn; tussen twee
        /// opeenvolgende shapes wordt puntsgewijs lineair geïnterpoleerd.
        /// Alle shapes moeten hetzelfde aantal punten hebben.
        /// </summary>
        public StaafShape[] Shapes { get; set; } = [];

        /// <summary> Gemak: de (start)vorm. </summary>
        public StaafShape Shape => Shapes[0];

        /// <summary> Doorsnede van één staaf As = π/4·Ø² [mm²]. </summary>
        public double As => Math.PI / 4 * Diameter * Diameter;
        public double BuigstraalMin => Diameter <= 16 ? 2.0 * Diameter : 3.5 * Diameter; // 8.1(N)

        /// <summary> Uitgeslagen lengte van de (start)vorm [mm]. </summary>
        public double UitgeslagenLengte => Shape.UitgeslagenLengte(Diameter);

       


        /// <summary> Basisvalidatie: diameter en shapes. Afgeleiden breiden dit uit. </summary>
        public virtual void Valideer()
        {
            if (Diameter <= 0)
            {
                // todo: dit is een fout in de invoer
                // misschien beter een warning?
                //throw new WarningException("Diameter moet groter dan 0 zijn???");
            }

            if (Shapes.Length < 1)
            {
                throw new InvalidOperationException("Minimaal 1 staafvorm verwacht.");
            }

            foreach (var shape in Shapes)
            {
                shape.Valideer();
            }

            if (Shapes.Any(s => s.Punten.Count != Shapes[0].Punten.Count))
            {
                throw new InvalidOperationException("Alle staafvormen moeten hetzelfde aantal punten hebben.");
            }
        }
    }
}
