namespace WorkTogetherly.Application.Common.Services;

public static class GeoCalculator
{
    // Haversine formula to calculate distance between two lat/lng points in kilometers.
    public static double DistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180)
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    // Deterministic offset (~100m max) so exact coordinates never leave the server.
    // Using the entity id (not random) means the same workspace always returns the same approximate position.
    // Primes 127 and 251 spread the values evenly across the [-100, 100] range.
    public static (double Lat, double Lng) ApproximateCoords(int id, double lat, double lng) =>
        (lat + ((id * 127) % 200 - 100) / 100_000.0,
         lng + ((id * 251) % 200 - 100) / 100_000.0);
}
