using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Models.Metadata
{
    public interface IModelMetadata<TModel>
    {
        IReadOnlyDictionary<string, PropertyMetadata<TModel>> Properties { get; }

        PropertyMetadata<TModel>? Get(string propertyName);
    }
}
