using MediatR;
using WorkTogetherly.Application.Materials.Common;

namespace WorkTogetherly.Application.Materials.GetAllMaterials;

public record GetAllMaterialsQuery : IRequest<List<MaterialResult>>;
