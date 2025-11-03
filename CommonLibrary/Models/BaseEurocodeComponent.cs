using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Components;

namespace CommonLibrary.Models
{
    /// <summary>
    /// Basis component voor alle Eurocode gerelateerde Blazor componenten.
    /// Zorgt ervoor dat wijzigingen in NumberFormatter.DefaultSignificantDigits automatisch een her-rendering veroorzaken.
    /// </summary>


    public abstract class BaseEurocodeComponent : ComponentBase, IDisposable
    {
        protected override void OnInitialized()
        {
            NumberFormatter.DefaultSignificantDigitsChanged += OnNumberFormatterChanged;
            base.OnInitialized();
        }

        private void OnNumberFormatterChanged(object? sender, int newValue)
        {
            // Forceer her-render in afgeleide componenten
            InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            NumberFormatter.DefaultSignificantDigitsChanged -= OnNumberFormatterChanged;
        }
    }


}



