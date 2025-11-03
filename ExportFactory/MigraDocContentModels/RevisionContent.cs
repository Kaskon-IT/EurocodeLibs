using System.ComponentModel;

namespace ExportFactory.MigraDocContentModels
{
    public class RevisionContent : INotifyPropertyChanged
    {
        private List<Revision> _revisions = new List<Revision>();


        public List<Revision> Revisions
        {
            get => _revisions;
            set
            {
                _revisions = value;
                OnPropertyChanged(nameof(Revisions));
            }
        }




        //public Dictionary<string, string> ColumnNames { get; set; } = new Dictionary<string, string>()
        //{
        //    { "Name", "Rev."},
        //    { "Date", "Datum" },
        //    { "Description", "Beschrijving" }
        //};

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }



}
