using CommonLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Interfaces
{
    public interface IBerekening<TInput, TResult>
    {
        string Naam { get; }
        TInput Input { get; set; }
        TResult? Result { get; }

        IReadOnlyList<TexFormula> Formules { get; }

        TResult Bereken();
    }

    
}
