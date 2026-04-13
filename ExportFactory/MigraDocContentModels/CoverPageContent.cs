using CommonLibrary.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace ExportFactory.MigraDocContentModels
{
    public class CoverPageContent : INotifyPropertyChanged
    {
        private string _title = "Untitled Document";
        private string _subtitle = "Subtitle";
        private string _projectNumber = "0000000";
        private string _companyName = "";
        private string _companyLogoPath = "";
        private string _documentNumber = "Document Number";
        private string _author = "Author Name";
        private string _svgLogoXml = "";


        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Subtitle
        {
            get => _subtitle;
            set
            {
                if (_subtitle != value)
                {
                    _subtitle = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string ProjectNumber
        {
            get => _projectNumber;
            set
            {
                if (_projectNumber != value)
                {
                    _projectNumber = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string CompanyName
        {
            get => _companyName;
            set
            {
                if (_companyName != value)
                {
                    _companyName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string CompanyLogoPath
        {
            get => _companyLogoPath;
            set
            {
                if (_companyLogoPath != value)
                {
                    _companyLogoPath = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string SvgLogoXml
        {
            get => _svgLogoXml;
            set
            {
                if (_svgLogoXml != value)
                {
                    _svgLogoXml = value;
                    NotifyPropertyChanged();
                }
            }
        }


        public string DocumentNumber
        {
            get => _documentNumber;
            set
            {
                if (_documentNumber != value)
                {
                    _documentNumber = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Author
        {
            get => _author;
            set
            {
                if (_author != value)
                {
                    _author = value;
                    NotifyPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public List<LabelWithStringValue> ProjectLabels { get; set; } = [];
        public List<LabelWithStringValue> DocumentLabels { get; set; } = [];


        public RevisionContent RevisionContent { get; set; } = new RevisionContent();

        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }



}
