using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Models
{
    [Obsolete("Use Formula instead.")]
    public class TexFormula : INotifyPropertyChanged
    {
        public string Titel { get; set; } = "";
        public string Tex { get; set; } = "";


        public string TexValues { get; set; } = "";
        public string TexWithWaarde => Waarde is not null ? $"{Tex} = {Waarde}": Tex;
        public string? Waarde { get; set; }

        public TexFormula() { }

        public TexFormula(string titel, string tex, string? waarde = null)
        {
            Titel = titel;
            Tex = tex;
            Waarde = waarde;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
