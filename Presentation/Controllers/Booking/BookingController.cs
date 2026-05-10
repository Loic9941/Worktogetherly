using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkTogetherly.Application.Bookings.CancelBooking;
using WorkTogetherly.Application.Bookings.CreateBooking;
using WorkTogetherly.Application.Bookings.GetUserBookings;
using WorkTogetherly.Application.Bookings.UpdateBookingArrivalTime;
using WorkTogetherly.Presentation.Models.Booking;

namespace WorkTogetherly.Presentation.Controllers.Booking
{
    [Route("api/bookings")]
    [Authorize]
    public class BookingController(IMediator mediator) : ApiController
    {
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBookingRequest request)
        {
            var userId = GetUserId();
            var command = new CreateBookingCommand(request.SlotId, userId, request.ArrivalTime, request.MaterialIds ?? []);
            var result = await mediator.Send(command);

            return result.Match(
                bookingId => Ok(new { Id = bookingId }),
                errors => Problem(errors));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var userId = GetUserId();
            var result = await mediator.Send(new CancelBookingCommand(id, userId));

            return result.Match(
                _ => NoContent(),
                errors => Problem(errors));
        }

        [HttpPatch("{id:int}/arrival-time")]
        public async Task<IActionResult> UpdateArrivalTime(int id, [FromBody] UpdateArrivalTimeRequest request)
        {
            var userId = GetUserId();
            var result = await mediator.Send(new UpdateBookingArrivalTimeCommand(id, userId, request.ArrivalTime));

            return result.Match(
                value => Ok(value),
                errors => Problem(errors));
        }

        [HttpGet("mine")]
        public async Task<IActionResult> GetMyBookings(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var userId = GetUserId();
            var dateFrom = from ?? DateTime.UtcNow.Date;
            var dateTo = to ?? dateFrom.AddDays(6);

            var query = new GetUserBookingsQuery(userId, dateFrom, dateTo);
            var result = await mediator.Send(query);

            return result.Match(
                value => Ok(value),
                errors => Problem(errors));
        }

    }
}
