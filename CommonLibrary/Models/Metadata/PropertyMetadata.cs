using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Models.Metadata
{


    public abstract class PropertyMetadata<TModel>
    {
        public required string PropertyName { get; init; }

        public string? Label { get; init; }

        public string? Description { get; init; }

        public string? Unit { get; init; }

        public int Order { get; init; }

        public Func<TModel, bool>? IsVisible { get; init; }

        public Func<TModel, bool>? IsEnabled { get; init; }

        public bool GetIsVisible(TModel model)
            => IsVisible?.Invoke(model) ?? true;

        public bool GetIsEnabled(TModel model)
            => IsEnabled?.Invoke(model) ?? true;
    }

   


    public sealed class NumericPropertyMetadata<TModel>
    : PropertyMetadata<TModel>
    {
        public Func<TModel, double?>? Min { get; init; }

        public Func<TModel, double?>? Max { get; init; }

        public Func<TModel, double>? Step { get; init; }

        public int Decimals { get; init; } = 0;

        public double? GetMin(TModel model)
            => Min?.Invoke(model);

        public double? GetMax(TModel model)
            => Max?.Invoke(model);

        public double GetStep(TModel model)
            => Step?.Invoke(model) ?? 1.0;

        // --- helpers for string
        public string? GetMinString(TModel model) => ToInvariantString(GetMin(model));

        public string? GetMaxString(TModel model)  => ToInvariantString(GetMax(model));

        public string GetStepString(TModel model)  => GetStep(model).ToString(CultureInfo.InvariantCulture);

        private static string? ToInvariantString(double? value) => value?.ToString(CultureInfo.InvariantCulture);


    }

    public interface IPropertyMetadataProvider<TModel>
    {
        IReadOnlyDictionary<string, PropertyMetadata<TModel>> PropertyMetadata { get; }
    }
}
