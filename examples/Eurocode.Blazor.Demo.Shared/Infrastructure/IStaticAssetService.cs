namespace Eurocode.Blazor.Demo.Shared;

public interface IStaticAssetService
{
    public Task<string?> GetAsync(string assetUrl, bool useCache = true);
}
