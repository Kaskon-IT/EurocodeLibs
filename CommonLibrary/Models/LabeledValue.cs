using System.ComponentModel;
using System.Globalization;
using System.Text.Json.Serialization;

namespace CommonLibrary.Models
{
    public class LabelWithStringValue : INotifyPropertyChanged
    {
        public LabelWithStringValue()
        {

        }

        public LabelWithStringValue(string label, string stringValue)
        {
            Label = label;
            StringValue = stringValue;
        }

        private string _label = string.Empty;
        private string _stringValue = string.Empty;

        public string Label
        {
            get => _label;
            set
            {
                if (_label != value)
                {
                    _label = value;
                    OnPropertyChanged(nameof(Label));
                }
            }
        }

        public string StringValue
        {
            get => _stringValue;
            set
            {
                if (_stringValue != value)
                {
                    _stringValue = value;
                    OnPropertyChanged(nameof(StringValue));
                }
            }
        }


        public override string ToString()
        {
            return $"{Label} : {StringValue}";
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName) =>
           PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }





    public class LabeledValue : INotifyPropertyChanged
    {
        private string _label = string.Empty;
        private object? _value = null;
        private string? _format = null;
        private string _nullDisplayText = "NULL";
        private string _cultureName = CultureInfo.InvariantCulture.Name;

        public string Label
        {
            get => _label;
            set
            {
                if (_label != value)
                {
                    _label = value;
                    OnPropertyChanged(nameof(Label));
                }
            }
        }

        public object? Value
        {
            get => _value;
            set
            {
                if (!IsAllowedType(value))
                    throw new InvalidOperationException($"Type '{value?.GetType().Name}' is not allowed in LabeledValue.");

                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                    OnPropertyChanged(nameof(ValueAsString));
                }
            }
        }

        public string? Format
        {
            get => _format;
            set
            {
                if (_format != value)
                {
                    _format = value;
                    OnPropertyChanged(nameof(Format));
                    OnPropertyChanged(nameof(ValueAsString));
                }
            }
        }

        public string NullDisplayText
        {
            get => _nullDisplayText;
            set
            {
                if (_nullDisplayText != value)
                {
                    _nullDisplayText = value;
                    OnPropertyChanged(nameof(NullDisplayText));
                    OnPropertyChanged(nameof(ValueAsString));
                }
            }
        }

        public string CultureName
        {
            get => _cultureName;
            set
            {
                if (_cultureName != value)
                {
                    _cultureName = value;
                    OnPropertyChanged(nameof(CultureName));
                    OnPropertyChanged(nameof(Culture));
                    OnPropertyChanged(nameof(ValueAsString));
                }
            }
        }

        [JsonIgnore]
        public CultureInfo Culture => CultureInfo.GetCultureInfo(CultureName);


        public LabeledValue(string label, object? value, string? format = null, string nullDisplayText = "NULL")
        {
            Label = label;
            Value = value;
            Format = format;
            NullDisplayText = nullDisplayText;
        }





        /// <summary>
        /// Geeft de waarde als geformatteerde string, rekening houdend met Format en null.
        /// </summary>
        public string ValueAsString
        {
            get
            {
                if (Value == null)
                    return NullDisplayText;

                if (Value is IFormattable formattable && !string.IsNullOrWhiteSpace(Format))
                    return formattable.ToString(Format, Culture);

                return Value.ToString() ?? NullDisplayText;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;


        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private static bool IsAllowedType(object? value)
        {
            if (value == null)
                return true;

            var type = value.GetType();

            return type == typeof(string) ||
                   type == typeof(int) ||
                   type == typeof(double) ||
                   type == typeof(decimal) ||
                   type == typeof(bool) ||
                   type == typeof(DateTime) ||
                   type == typeof(float) ||
                   type == typeof(long) ||
                   type == typeof(short) ||
                   type.IsEnum;
        }


        public override string ToString()
        {
            return $"{Label} : {ValueAsString}";
        }
    }

}
