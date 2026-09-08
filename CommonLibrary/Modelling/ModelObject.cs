using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Modelling
{
    /// <summary>
    /// Identifies a non-physical model object that is functionally owned by a part.
    /// </summary>
    public interface IPartOwnedModelObject
    {
        Guid? PartId { get; set; }
    }

    public abstract class ModelObject
    {
        /// <summary>
        /// Gets the stable identifier of this model object.
        /// </summary>
        public Guid Id { get; init; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the identifier of the parent in the model hierarchy.
        /// A <see langword="null"/> value identifies a root object.
        /// </summary>
        public Guid? ParentId { get; set; }
        
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

    /// <summary>
    /// Represents a grouping object that can contain assemblies and parts.
    /// The optional main part must be one of the assembly's direct child parts.
    /// </summary>
    public class Assembly : ModelObject
    {
        public Guid? MainPartId { get; set; }
    }

    public class Beam : Part
    {
        public Punt3D StartPoint { get; set; }
        public Punt3D EndPoint { get; set; }
    }

    /// <summary>
    /// Represents a slab part in the model.
    /// </summary>
    public class Slab : Part
    {
    }


    public class Load : ModelObject, IPartOwnedModelObject
    {
        public Guid? PartId { get; set; }

        [Obsolete("Use PartId instead.")]
        [System.Text.Json.Serialization.JsonIgnore]
        public Guid FatherId
        {
            get => PartId ?? Guid.Empty;
            set => PartId = value;
        }

    }

    public class PointLoad : Load
    {
        public Punt3D Direction { get; set; }

        public Punt3D Point { get; set; }
        public Punt3D Force { get; set; }
    }

    
    




}
