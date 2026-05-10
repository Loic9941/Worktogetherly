namespace WorkTogetherly.Shared.Models
{
    public record WorkspaceDto(
        int Id,
        Guid? UserId,
        string Name,
        string Description,
        string Address,
        double Latitude,
        double Longitude,
        int Capacity,
        bool IsActive,
        DateTime CreatedAt,
        List<WorkspaceMaterialDto> Materials,
        List<WorkspaceRuleDto> Rules,
        string? PhotoPath);

    public class WorkspaceFormModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Capacity { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public List<MaterialFormItem> Materials { get; set; } = [];
        public List<RuleFormItem> Rules { get; set; } = [];
    }

    public class MaterialFormItem
    {
        public int MaterialId { get; set; }
        public bool Enabled { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class RuleFormItem
    {
        public int RuleId { get; set; }
        public bool Enabled { get; set; }
    }

    public record SlotDto(
        int Id,
        int WorkspaceId,
        DateTime StartDateTime,
        DateTime EndDateTime,
        int Capacity,
        int AvailablePlaces,
        List<AttendeeDto> Attendees);

    public record AttendeeDto(string FirstName, string LastInitial, TimeOnly ArrivalTime);

    public record CitySuggestion(string DisplayName, double Latitude, double Longitude);

    public record WorkspaceSearchResultDto(
        int Id,
        string Name,
        string Description,
        double Latitude,
        double Longitude,
        int Capacity,
        string? PhotoPath,
        double? DistanceKm,
        List<WorkspaceMaterialDto> Materials,
        List<WorkspaceRuleDto> Rules);

    public record AmenityFilter(List<int> MaterialIds, List<int> RuleIds);

    public class WorkspaceSearchRequest
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double RadiusKm { get; set; } = 20;
        public DateOnly Date { get; set; }
    }

    public class SlotFormModel
    {
        public DateTime? Date { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public int Capacity { get; set; } = 1;
    }

    public record WorkspaceDetailDto(
        int Id,
        string Name,
        string Description,
        double ApproxLatitude,
        double ApproxLongitude,
        double AverageRating,
        int ReviewCount,
        List<WorkspaceMaterialDto> Materials,
        List<WorkspaceRuleDto> Rules,
        List<SlotDetailDto> Slots);

    public record SlotDetailDto(
        int Id,
        DateTime StartDateTime,
        DateTime EndDateTime,
        int Capacity,
        int AvailablePlaces,
        bool UserHasBooked,
        List<SlotMaterialAvailabilityDto> MaterialAvailabilities);
}
