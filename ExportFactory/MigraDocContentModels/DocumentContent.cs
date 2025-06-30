using MigraDoc.DocumentObjectModel;
using System.ComponentModel;

namespace ExportFactory.MigraDocContentModels
{
    public class DocumentContent
    {

        public DocumentInfo DocumentInfo { get; set; } = new DocumentInfo()
        {
            Author = "Kaskon",
            Title = "Title",
            Comment = "",
            Keywords = "prefabbeton, trappen en bordessen",
            Subject = "",


        };

        public Color AccentColor { get; set; } = Colors.Teal;

        public Color HeaderBackgroundColor { get; set; } = Colors.Transparent;
        public Color HeaderLineColor { get; set; } = Colors.Teal;
        public Color HeaderColor { get; set; } = Colors.Black;

        public Color FooterBackgroundColor { get; set; } = Colors.Transparent;
        public Color FooterLineColor { get; set; } = Colors.Teal;
        public Color FooterColor { get; set; } = Colors.Black;

        public RevisionContent Revisions { get; set; } = new();
        public Font Font { get; set; } = new Font("Segoe UI Emoji", 9);
        public CoverPageContent CoverPage { get; set; } = new CoverPageContent();
        public PageHeaderContent PageHeader { get; set; } = new PageHeaderContent();
        public PageFooterContent PageFooter { get; set; } = new PageFooterContent();
        public List<SectionContent> Sections { get; set; } = new List<SectionContent>();
        public TableOfContentsContent TableOfContents { get; set; }

        public PageMarginAndPageNumberSettingsEnum? PageMarginSetting { get; set; } = PageMarginAndPageNumberSettingsEnum.MargeLinks_PaginaNummerRechts;


        public enum PageMarginAndPageNumberSettingsEnum
        {
            [Description("Alles gecentreerd")]
            Gecentreerd,
            [Description("Marge links")]
            MargeLinks_PaginaNummerRechts,
            [Description("Even/Oneven gespiegeld")]
            EvenOnevenGespiegeld,

        }

    }


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

    public class Revision : INotifyPropertyChanged
    {
        private string _name = "";
        private string _description = "";
        private DateTime? _date = DateTime.Now;

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }


        public DateTime? Date
        {
            get => _date;
            set
            {
                if (_date != value)
                {
                    _date = value;
                    OnPropertyChanged(nameof(Date));
                }
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return $"{Name} - {Date?.ToShortDateString()} - {Description}";
        }


    }






}
