namespace CommonLibrary
{
    /// <summary>
    /// Voor het maken van referentie types van value types.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Ref<T>
    {
        private T _value;
        public event Action<T>? ValueChanged;

        public T Value
        {
            get => _value;
            set
            {
                if (!EqualityComparer<T>.Default.Equals(_value, value))
                {
                    _value = value;
                    ValueChanged?.Invoke(value);
                }
            }
        }

        public Ref(T value) => _value = value;
    }

}
