namespace WorkTogetherly.Domain.Entities
{
    public class Workspace
    {
        public int Id { get; private set; }
        public Guid UserId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string Address { get; private set; } = string.Empty;
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public int Capacity { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string? PhotoPath { get; private set; }

        public User Owner { get; private set; } = null!;
        public ICollection<Slot> Slots { get; private set; } = [];
        public ICollection<WorkspaceMaterial> WorkspaceMaterials { get; private set; } = [];
        public ICollection<WorkspaceRule> WorkspaceRules { get; private set; } = [];
        public ICollection<Review> Reviews { get; private set; } = [];

        private Workspace() { }

        public static Workspace Create(
            Guid userId,
            string name,
            string description,
            string address,
            double latitude,
            double longitude,
            int capacity,
            bool isActive)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
            return new Workspace
            {
                UserId = userId,
                Name = name,
                Description = description,
                Address = address,
                Latitude = latitude,
                Longitude = longitude,
                Capacity = capacity,
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void Update(
            string name,
            string description,
            string address,
            double latitude,
            double longitude,
            int capacity,
            bool isActive)
        {
            Name = name;
            Description = description;
            Address = address;
            Latitude = latitude;
            Longitude = longitude;
            Capacity = capacity;
            IsActive = isActive;
        }

        public string? RemovePhoto()
        {
            var previous = PhotoPath;
            PhotoPath = null;
            return previous;
        }

        public string? ReplacePhoto(string newPath)
        {
            var previous = PhotoPath;
            PhotoPath = newPath;
            return previous;
        }

        public void ReplaceMaterials(IEnumerable<(int materialId, int quantity)> items)
        {
            WorkspaceMaterials.Clear();
            foreach (var (materialId, quantity) in items)
                WorkspaceMaterials.Add(WorkspaceMaterial.Create(Id, materialId, quantity));
        }

        public void ReplaceRules(IEnumerable<int> ruleIds)
        {
            WorkspaceRules.Clear();
            foreach (var ruleId in ruleIds)
                WorkspaceRules.Add(WorkspaceRule.Create(Id, ruleId));
        }
    }
}
