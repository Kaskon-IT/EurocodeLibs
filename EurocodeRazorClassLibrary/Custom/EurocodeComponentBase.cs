namespace EurocodeRazorClassLibrary.Custom
{
    using Microsoft.AspNetCore.Components;

    public class EurocodeComponentBase : ComponentBase
    {
        [Parameter] public EventCallback OnStateChanged { get; set; }

        protected async Task NotifyParentAsync()
        {
            if (OnStateChanged.HasDelegate)
                await OnStateChanged.InvokeAsync();
        }
    }

}
