using CommonLibrary;

namespace EurocodeRazorClassLibrary.Custom
{
    /// <summary>
    /// Bevat de padstring (Path) naar de property én de bijbehorende BaseEurocodeContext (of null).
    /// </summary>
    public record ContextNodeInfo(string Path, BaseEurocodeContext? Context);


    /// <summary>
    /// Beschrijft één gevonden BaseEurocodeContext-eigenschap:
    /// - Path    = bezigaan met welke eigenschap (bv. "BetonContext" of "BendingResult1.GebruikstBetonContext")
    /// - Context = de daadwerkelijke BaseEurocodeContext-instantie (kan null zijn)
    /// </summary>
    public record EContextNodeInfo(string Path, BaseEurocodeContext? Context, int Depth);
}
