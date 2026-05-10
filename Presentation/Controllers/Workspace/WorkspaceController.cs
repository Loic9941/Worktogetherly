using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkTogetherly.Application.Workspaces.CreateWorkspace;
using WorkTogetherly.Application.Workspaces.GetMyWorkspace;
using WorkTogetherly.Application.Workspaces.GetWorkspaceDetails;
using WorkTogetherly.Application.Workspaces.Search;
using WorkTogetherly.Application.Workspaces.DeleteWorkspacePhoto;
using WorkTogetherly.Application.Workspaces.UpdateWorkspace;
using WorkTogetherly.Application.Workspaces.UploadWorkspacePhoto;
using WorkTogetherly.Application.Reviews.GetWorkspaceReviews;
using WorkTogetherly.Presentation.Models.Workspace;

namespace WorkTogetherly.Presentation.Controllers.Workspace
{
    [Route("api/workspaces")]
    [Authorize]
    public class WorkspaceController(IMediator mediator) : ApiController
    {
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] double latitude,
            [FromQuery] double longitude,
            [FromQuery] DateOnly date,
            [FromQuery] double radiusKm = 20)
        {
            var userId = GetUserId();

            var query = new SearchWorkspacesQuery(latitude, longitude, radiusKm, date, userId);
            var result = await mediator.Send(query);

            return result.Match(
                value => Ok(value),
                errors => Problem(errors));
        }

        [HttpGet("{id:int}/details")]
        public async Task<IActionResult> GetDetails(int id, [FromQuery] DateOnly date)
        {
            var userId = GetUserId();

            var query = new GetWorkspaceDetailsQuery(id, userId, date);
            var result = await mediator.Send(query);

            return result.Match(
                value => Ok(value),
                errors => Problem(errors));
        }

        [HttpGet("mine")]
        public async Task<IActionResult> GetMyWorkspace()
        {
            var userId = GetUserId();

            var query = new GetMyWorkspaceQuery(userId);
            var result = await mediator.Send(query);

            return result.Match(
                value => value is null ? NotFound() : Ok(value),
                errors => Problem(errors));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateWorkspaceRequest request)
        {
            var userId = GetUserId();

            var command = new CreateWorkspaceCommand(
                userId,
                request.Name,
                request.Description,
                request.Address,
                request.Latitude,
                request.Longitude,
                request.Capacity,
                request.IsActive,
                request.Materials,
                request.RuleIds);

            var result = await mediator.Send(command);

            return result.Match(
                value => CreatedAtAction(nameof(GetMyWorkspace), value),
                errors => Problem(errors));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateWorkspaceRequest request)
        {
            var userId = GetUserId();

            var command = new UpdateWorkspaceCommand(
                id,
                userId,
                request.Name,
                request.Description,
                request.Address,
                request.Latitude,
                request.Longitude,
                request.Capacity,
                request.IsActive,
                request.Materials,
                request.RuleIds);

            var result = await mediator.Send(command);

            return result.Match(
                value => Ok(value),
                errors => Problem(errors));
        }

        [HttpPost("{id:int}/photo")]
        [RequestFormLimits(MultipartBodyLengthLimit = 5_242_880)]
        public async Task<IActionResult> UploadPhoto(int id, IFormFile file)
        {
            var userId = GetUserId();

            await using var stream = file.OpenReadStream();

            var command = new UploadWorkspacePhotoCommand(id, userId, stream, file.FileName, file.Length);
            var result = await mediator.Send(command);

            return result.Match(
                value => Ok(value),
                errors => Problem(errors));
        }

        [HttpGet("{id:int}/reviews")]
        public async Task<IActionResult> GetReviews(int id)
        {
            var query = new GetWorkspaceReviewsQuery(id);
            var result = await mediator.Send(query);

            return result.Match(
                value => Ok(value),
                errors => Problem(errors));
        }

        [HttpDelete("{id:int}/photo")]
        public async Task<IActionResult> DeletePhoto(int id)
        {
            var userId = GetUserId();

            var command = new DeleteWorkspacePhotoCommand(id, userId);
            var result = await mediator.Send(command);

            return result.Match(
                value => Ok(value),
                errors => Problem(errors));
        }

    }
}
