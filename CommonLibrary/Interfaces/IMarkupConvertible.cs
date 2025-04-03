using Microsoft.AspNetCore.Components;

namespace CommonLibrary.Interfaces
{
    public interface IMarkupConvertible
    {
        MarkupString ToMarkupString();

    }
}
