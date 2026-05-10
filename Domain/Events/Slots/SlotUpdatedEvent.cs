namespace WorkTogetherly.Domain.Events.Slots;

public record SlotUpdatedEvent(int SlotId, int WorkspaceId) : IDomainEvent;
