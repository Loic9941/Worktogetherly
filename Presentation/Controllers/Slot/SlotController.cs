using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkTogetherly.Application.Slots.CancelSlot;
using WorkTogetherly.Application.Slots.CreateSlot;
using WorkTogetherly.Application.Slots.GetSlotsByWorkspace;
using WorkTogetherly.Application.Slots.UpdateSlot;
using WorkTogetherly.Presentation.Models.Slot;

namespace WorkTogetherly.Presentation.Controllers.Slot
{
    [Route("api/workspaces/{workspaceId:int}/slots")]
    [Authorize]
    public class SlotController(IMediator mediator) : ApiController
    {
        [HttpGet]
        public async Task<IActionResult> GetSlots(int workspaceId)
        {
            var query = new GetSlotsByWorkspaceQuery(workspaceId);
            var result = await mediator.Send(query);

            return result.Match(
                value => Ok(value),
                errors => Problem(errors));
        }

        [HttpPost]
        public async Task<IActionResult> Create(int workspaceId, [FromBody] CreateSlotRequest request)
        {
            var userId = GetUserId();
            var command = new CreateSlotCommand(workspaceId, userId, request.StartDateTime, request.EndDateTime, request.Capacity);
            var result = await mediator.Send(command);

            return result.Match(
                value => CreatedAtAction(nameof(GetSlots), new { workspaceId }, value),
                errors => Problem(errors));
        }

        [HttpPut("{slotId:int}")]
        public async Task<IActionResult> Update(int workspaceId, int slotId, [FromBody] UpdateSlotRequest request)
        {
            var userId = GetUserId();
            var command = new UpdateSlotCommand(slotId, workspaceId, userId, request.StartDateTime, request.EndDateTime, request.Capacity);
            var result = await mediator.Send(command);

            return result.Match(
                value => Ok(value),
                errors => Problem(errors));
        }

        [HttpDelete("{slotId:int}")]
        public async Task<IActionResult> Cancel(int workspaceId, int slotId)
        {
            var userId = GetUserId();
            var command = new CancelSlotCommand(slotId, workspaceId, userId);
            var result = await mediator.Send(command);

            return result.Match(
                _ => NoContent(),
                errors => Problem(errors));
        }
    }
}
