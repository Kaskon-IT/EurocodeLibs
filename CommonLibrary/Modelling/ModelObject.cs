using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Modelling
{
    public abstract class ModelObject
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string? DisplayName { get; set; }

        public virtual string DisplayText =>
            !string.IsNullOrWhiteSpace(DisplayName)
                ? DisplayName
                : Id.ToString();
    }

    public class Numbering
    {
        public string Prefix { get; set; } = "POS";
        public int Number { get; set; }
        public string Suffix { get; set; } = "";
        public string Separator { get; set; } = ".";

        public override string ToString()
        {
            return string.Join(
                Separator,
                new[] { Prefix, Number.ToString(), Suffix }
                    .Where(x => !string.IsNullOrWhiteSpace(x))
            );
        }
    }

   

    public abstract class Part : ModelObject
    {
        public Numbering PartNumber { get; set; } = new Numbering();
        public string Name { get; set; } = "";
        public string Material { get; set; } = "";
    }

    public class Beam : Part
    {
        public Punt3D StartPoint { get; set; }
        public Punt3D EndPoint { get; set; }
    }


}
