using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Web.Client.Services
{
    /// <summary>
    /// Utility class for calling JS function declared in index.html
    /// </summary>
    public class JsInterop : IJsInterop
    {
        private readonly IJSRuntime jsRuntime;

        public JsInterop(IJSRuntime jsRuntime)
        {
            this.jsRuntime = jsRuntime;
        }
        
        private class StringHolder
        {
            public string Content { get; set; }
        }

        public async Task<string> GetFileData(ElementRef fileInputRef)
        {
            var result = await jsRuntime.InvokeAsync<StringHolder>("getFileData", fileInputRef);

            return result.Content;
        }
    }
}
