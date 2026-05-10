using WorkTogetherly.Domain.Entities;

namespace TestWorkTogetherly.Helpers;

internal static class EntityFactory
{
    public static Workspace MakeWorkspace(int id = 1, Guid? ownerId = null, int capacity = 10, bool isActive = true)
    {
        var ws = Instantiate<Workspace>();
        Set(ws, "Id", id);
        Set(ws, "UserId", ownerId ?? Guid.NewGuid());
        Set(ws, "Name", "Test Workspace");
        Set(ws, "Address", "1 rue Test");
        Set(ws, "Capacity", capacity);
        Set(ws, "IsActive", isActive);
        return ws;
    }

    public static Slot MakeSlot(
        int id = 1,
        int workspaceId = 1,
        DateTime? start = null,
        DateTime? end = null,
        int capacity = 5,
        Workspace? workspace = null,
        bool cancelled = false)
    {
        var s = start ?? DateTime.UtcNow.AddDays(1);
        var e = end ?? s.AddHours(8);
        var slot = Instantiate<Slot>();
        Set(slot, "Id", id);
        Set(slot, "WorkspaceId", workspaceId);
        Set(slot, "StartDateTime", s);
        Set(slot, "EndDateTime", e);
        Set(slot, "Capacity", capacity);
        Set(slot, "Workspace", workspace ?? MakeWorkspace());
        if (cancelled) Set(slot, "CancelledAt", (DateTime?)DateTime.UtcNow.AddHours(-1));
        return slot;
    }

    public static Booking MakeBooking(
        int id = 1,
        int slotId = 1,
        Guid? userId = null,
        TimeOnly? arrivalTime = null,
        Slot? slot = null,
        bool cancelled = false)
    {
        var booking = Instantiate<Booking>();
        Set(booking, "Id", id);
        Set(booking, "SlotId", slotId);
        Set(booking, "UserId", userId ?? Guid.NewGuid());
        Set(booking, "ArrivalTime", arrivalTime ?? new TimeOnly(9, 0));
        Set(booking, "CreatedAt", DateTime.UtcNow.AddDays(-1));
        Set(booking, "Slot", slot ?? MakeSlot());
        if (cancelled) Set(booking, "CancelledAt", (DateTime?)DateTime.UtcNow.AddHours(-1));
        return booking;
    }

    public static Review MakeReview(
        int id = 1,
        int bookingId = 1,
        Guid? reviewerId = null,
        int workspaceId = 1,
        int rating = 4,
        string comment = "Bien")
    {
        var review = Instantiate<Review>();
        Set(review, "Id", id);
        Set(review, "BookingId", bookingId);
        Set(review, "ReviewerId", reviewerId ?? Guid.NewGuid());
        Set(review, "WorkspaceId", workspaceId);
        Set(review, "Rating", rating);
        Set(review, "Comment", comment);
        Set(review, "CreatedAt", DateTime.UtcNow.AddDays(-1));
        return review;
    }

    public static Material MakeMaterial(int id = 1, string name = "Test Material")
    {
        var material = Instantiate<Material>();
        Set(material, "Id", id);
        Set(material, "Name", name);
        return material;
    }

    public static Rule MakeRule(int id = 1, string name = "Test Rule")
    {
        var rule = Instantiate<Rule>();
        Set(rule, "Id", id);
        Set(rule, "Name", name);
        return rule;
    }

    public static WorkspaceMaterial MakeWorkspaceMaterial(int workspaceId = 1, int materialId = 1, int quantity = 3, Material? material = null)
    {
        var wm = Instantiate<WorkspaceMaterial>();
        Set(wm, "WorkspaceId", workspaceId);
        Set(wm, "MaterialId", materialId);
        Set(wm, "Quantity", quantity);
        Set(wm, "Material", material ?? MakeMaterial(materialId));
        return wm;
    }

    public static WorkspaceRule MakeWorkspaceRule(int workspaceId = 1, int ruleId = 1, Rule? rule = null)
    {
        var wr = Instantiate<WorkspaceRule>();
        Set(wr, "WorkspaceId", workspaceId);
        Set(wr, "RuleId", ruleId);
        Set(wr, "Rule", rule ?? MakeRule(ruleId));
        return wr;
    }

    public static BookingMaterial MakeBookingMaterial(int bookingId = 1, int materialId = 1, Material? material = null)
    {
        var bm = Instantiate<BookingMaterial>();
        Set(bm, "BookingId", bookingId);
        Set(bm, "MaterialId", materialId);
        Set(bm, "Material", material ?? MakeMaterial(materialId));
        return bm;
    }

    private static T Instantiate<T>() =>
        (T)Activator.CreateInstance(typeof(T), nonPublic: true)!;

    private static void Set<T>(T obj, string property, object? value) =>
        typeof(T).GetProperty(property)!.SetValue(obj, value);
}
