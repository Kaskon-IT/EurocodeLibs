using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Models.Metadata
{

    public static class ModelMetadataExtensions
    {
        public static NumericPropertyMetadata<TModel>? TryGetNumeric<TModel>(
    this IModelMetadata<TModel> metadata,
    string propertyName)
        {
            return metadata.Get(propertyName)
                as NumericPropertyMetadata<TModel>;
        }


        //public static NumericPropertyMetadata<TModel> GetNumeric<TModel>(
        //    this IModelMetadata<TModel> metadata,
        //    string propertyName)
        //{
        //    if (metadata.Get(propertyName)
        //        is NumericPropertyMetadata<TModel> numeric)
        //    {
        //        return numeric;
        //    }

        //    throw new InvalidOperationException(
        //        $"Geen numerieke metadata gevonden voor " +
        //        $"'{typeof(TModel).Name}.{propertyName}'.");
        //}
    }
}
