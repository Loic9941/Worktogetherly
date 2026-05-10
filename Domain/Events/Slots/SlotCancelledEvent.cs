namespace WorkTogetherly.Domain.Events.Slots;

public record SlotCancelledEvent(int SlotId, int WorkspaceId) : IDomainEvent;
