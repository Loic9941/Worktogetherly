namespace WorkTogetherly.Domain.Entities;

public class BookingMaterial
{
    public int BookingId { get; private set; }
    public int MaterialId { get; private set; }

    public Booking Booking { get; private set; } = null!;
    public Material Material { get; private set; } = null!;

    private BookingMaterial() { }

    public static BookingMaterial Create(int bookingId, int materialId) =>
        new() { BookingId = bookingId, MaterialId = materialId };
}
