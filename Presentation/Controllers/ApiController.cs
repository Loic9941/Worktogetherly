using Microsoft.AspNetCore.Mvc;

namespace WorkTogetherly.Presentation.Controllers
{
    using ErrorOr;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using System.Security.Claims;

    [ApiController]
    public abstract class ApiController : ControllerBase
    {
        protected IActionResult Problem(List<Error> errors)
        {
            if (errors.All(e => e.Type == ErrorType.Validation))
            {
                var modelStateDictionary = new ModelStateDictionary();
                errors.ForEach(e => modelStateDictionary.AddModelError(e.Code, e.Description));
                return ValidationProblem(modelStateDictionary);
            }

            // For simplicity, we return the first error.
            var firstError = errors[0];

            var statusCode = firstError.Type switch
            {
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };

            return Problem(statusCode: statusCode, detail: firstError.Description);
        }

        private bool TryGetUserId(out Guid userId)
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub");
            return Guid.TryParse(claim, out userId);
        }

        // Use this on [Authorize] endpoints — [Authorize] already guarantees the claim is present.
        protected Guid GetUserId()
        {
            TryGetUserId(out var userId);
            return userId;
        }
    }
}
