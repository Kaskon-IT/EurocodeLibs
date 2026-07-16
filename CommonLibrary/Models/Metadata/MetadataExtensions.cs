using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Models.Metadata
{
    public static class PropertyMetadataProviderExtensions
    {
        public static NumericPropertyMetadata<TModel> GetNumericMetadata<TModel>(
            this IPropertyMetadataProvider<TModel> provider,
            string propertyName)
        {
            if (!provider.PropertyMetadata.TryGetValue(propertyName, out var metadata))
            {
                throw new InvalidOperationException(
                    $"Geen metadata gevonden voor property '{propertyName}'.");
            }

            if (metadata is not NumericPropertyMetadata<TModel> numericMetadata)
            {
                throw new InvalidOperationException(
                    $"Metadata voor property '{propertyName}' is niet numeriek.");
            }

            return numericMetadata;
        }
    }
}
