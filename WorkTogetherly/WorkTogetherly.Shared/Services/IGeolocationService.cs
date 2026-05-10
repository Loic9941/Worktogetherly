namespace WorkTogetherly.Shared.Services;

public interface IGeolocationService
{
    Task<(double Latitude, double Longitude)?> GetCurrentLocationAsync();
}
