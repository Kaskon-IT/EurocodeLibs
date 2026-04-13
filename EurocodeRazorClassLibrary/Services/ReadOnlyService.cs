namespace EurocodeRazorClassLibrary.Services
{
    public class ReadOnlyService
    {
        private Dictionary<string, bool> _readOnlyFields = [];

        public void SetReadOnly(string fieldName, bool isReadOnly)
        {
            _readOnlyFields[fieldName] = isReadOnly;
        }

        public void SetReadOnlyFields(Dictionary<string, bool> fields)
        {
            _readOnlyFields = new(fields);
        }

        public bool IsFieldReadOnly(string fieldName) =>
            _readOnlyFields.TryGetValue(fieldName, out var isReadOnly) && isReadOnly;
    }

}
