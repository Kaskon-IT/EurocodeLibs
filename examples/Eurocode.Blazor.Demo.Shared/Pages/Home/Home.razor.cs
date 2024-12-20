using System.Reflection;

namespace Eurocode.Blazor.Demo.Shared.Pages.Home;
public partial class Home
{
    private string? _version;

    protected override void OnInitialized()
    {
        _version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;

    }
}
