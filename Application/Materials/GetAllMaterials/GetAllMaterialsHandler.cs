using MediatR;
using WorkTogetherly.Application.Materials.Common;
using WorkTogetherly.Domain.Interfaces;

namespace WorkTogetherly.Application.Materials.GetAllMaterials;

public class GetAllMaterialsHandler(IMaterialRepository materialRepository) : IRequestHandler<GetAllMaterialsQuery, List<MaterialResult>>
{
    public async Task<List<MaterialResult>> Handle(GetAllMaterialsQuery request, CancellationToken cancellationToken)
    {
        var materials = await materialRepository.GetAllAsync(cancellationToken);
        return materials.Select(m => new MaterialResult(m.Id, m.Name)).ToList();
    }
}
