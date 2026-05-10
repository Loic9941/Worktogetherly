using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkTogetherly.Application.Materials.GetAllMaterials;
using WorkTogetherly.Application.Rules.GetAllRules;

namespace WorkTogetherly.Presentation.Controllers.Amenity
{
    [Route("api/materials")]
    [Authorize]
    public class MaterialsController(IMediator mediator) : ApiController
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await mediator.Send(new GetAllMaterialsQuery());
            return Ok(result);
        }
    }

    [Route("api/rules")]
    [Authorize]
    public class RulesController(IMediator mediator) : ApiController
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await mediator.Send(new GetAllRulesQuery());
            return Ok(result);
        }
    }
}
