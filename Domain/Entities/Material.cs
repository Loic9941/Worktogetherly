namespace WorkTogetherly.Domain.Entities;

public class Material
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    public ICollection<WorkspaceMaterial> WorkspaceMaterials { get; private set; } = [];

    private Material() { }

    public static Material Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Material { Name = name };
    }
}
