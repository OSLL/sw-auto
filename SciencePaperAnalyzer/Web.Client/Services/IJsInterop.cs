using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Web.Client.Services
{
    interface IJsInterop
    {
        Task<string> GetFileData(ElementRef fileInputRef);
    }
}
