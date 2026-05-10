using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkTogetherly.Application.Reviews.CreateReview;
using WorkTogetherly.Application.Reviews.GetReviewByBooking;
using WorkTogetherly.Application.Reviews.UpdateReview;
using WorkTogetherly.Presentation.Models.Review;

namespace WorkTogetherly.Presentation.Controllers.Review
{
    [Route("api/reviews")]
    [Authorize]
    public class ReviewController(IMediator mediator) : ApiController
    {
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReviewRequest request)
        {
            var userId = GetUserId();
            var command = new CreateReviewCommand(request.BookingId, userId, request.Rating, request.Comment);
            var result = await mediator.Send(command);

            return result.Match(
                value => Ok(value),
                errors => Problem(errors));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateReviewRequest request)
        {
            var userId = GetUserId();
            var command = new UpdateReviewCommand(id, userId, request.Rating, request.Comment);
            var result = await mediator.Send(command);

            return result.Match(
                value => Ok(value),
                errors => Problem(errors));
        }

        [HttpGet("booking/{bookingId:int}")]
        public async Task<IActionResult> GetByBooking(int bookingId)
        {
            var query = new GetReviewByBookingQuery(bookingId);
            var result = await mediator.Send(query);

            return result.Match(
                value => value is null ? NotFound() : Ok(value),
                errors => Problem(errors));
        }
    }
}
