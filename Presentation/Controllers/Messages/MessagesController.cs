using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkTogetherly.Application.Messages.GetUserMessages;
using WorkTogetherly.Application.Messages.MarkMessageAsRead;

namespace WorkTogetherly.Presentation.Controllers.Messages;

[Route("api/messages")]
[Authorize]
public class MessagesController(IMediator mediator) : ApiController
{
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine()
    {
        var userId = GetUserId();
        var result = await mediator.Send(new GetUserMessagesQuery(userId));
        return result.Match(value => Ok(value), errors => Problem(errors));
    }

    [HttpPatch("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = GetUserId();
        var result = await mediator.Send(new MarkMessageAsReadCommand(id, userId));
        return result.Match(_ => NoContent(), errors => Problem(errors));
    }
}
