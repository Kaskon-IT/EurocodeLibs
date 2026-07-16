using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Models.Metadata
{
    public sealed class ModelMetadata<TModel>
    : IModelMetadata<TModel>
    {
        private readonly IReadOnlyDictionary<
            string,
            PropertyMetadata<TModel>> _properties;

        public ModelMetadata(
            IReadOnlyDictionary<string, PropertyMetadata<TModel>> properties)
        {
            _properties = properties;
        }

        public IReadOnlyDictionary<string, PropertyMetadata<TModel>> Properties
            => _properties;

        public PropertyMetadata<TModel>? Get(string propertyName)
        {
            return _properties.TryGetValue(propertyName, out var metadata)
                ? metadata
                : null;
        }
    }
}
