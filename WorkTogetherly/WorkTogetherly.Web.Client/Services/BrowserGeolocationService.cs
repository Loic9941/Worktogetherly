using Microsoft.JSInterop;
using WorkTogetherly.Shared.Services;

namespace WorkTogetherly.Web.Client.Services;

internal class BrowserGeolocationService(IJSRuntime js) : IGeolocationService
{
    public async Task<(double Latitude, double Longitude)?> GetCurrentLocationAsync()
    {
        try
        {
            var result = await js.InvokeAsync<GeolocationResult>("getCurrentPosition");
            return (result.Latitude, result.Longitude);
        }
        catch
        {
            return null;
        }
    }

    private record GeolocationResult(double Latitude, double Longitude);
}
