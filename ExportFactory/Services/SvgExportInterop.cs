using Microsoft.JSInterop;

namespace ExportFactory.Services
{
    public class SvgExportInterop
    {
        private readonly IJSRuntime _jsRuntime;

        public SvgExportInterop(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<string> RenderSvgToTempFileAsync(string svgXml, int widthPx = 1200, int heightPx = 800)
        {
            var dotNetRef = DotNetObjectReference.Create(this);
            _tcs = new TaskCompletionSource<string>();

            await _jsRuntime.InvokeVoidAsync("window.svgHelpers.saveSvgToTemp", svgXml, widthPx, heightPx, dotNetRef);

            // wacht op callback uit JS
            return await _tcs.Task;
        }

        private TaskCompletionSource<string>? _tcs;

        [JSInvokable]
        public Task ReceivePngFromClient(string base64)
        {
            var bytes = Convert.FromBase64String(base64);
            var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.png");
            File.WriteAllBytes(tempPath, bytes);
            _tcs?.SetResult(tempPath);
            return Task.CompletedTask;
        }
    }

}
