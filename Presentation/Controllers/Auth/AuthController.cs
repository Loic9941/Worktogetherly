using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkTogetherly.Application.Users.GetCurrentUser;
using WorkTogetherly.Application.Users.Login;
using WorkTogetherly.Application.Users.Logout;
using WorkTogetherly.Application.Users.RefreshToken;
using WorkTogetherly.Application.Users.Register;
using WorkTogetherly.Application.Users.Shared;
using WorkTogetherly.Application.Users.ChangePassword;
using WorkTogetherly.Application.Users.UpdateUser;
using WorkTogetherly.Application.Users.UploadUserPhoto;
using WorkTogetherly.Application.Users.DeleteUserPhoto;
using WorkTogetherly.Application.Users.ForgotPassword;
using WorkTogetherly.Application.Users.ResetPassword;
using WorkTogetherly.Presentation.Models.Auth;

namespace WorkTogetherly.Presentation.Controllers.Auth
{
    [Route("api/[controller]")]
    public class AuthController(IMediator mediator) : ApiController
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var command = new RegisterCommand(request.Email, request.Password, request.FirstName, request.LastName);
            var result = await mediator.Send(command);

            return result.Match(
                value => { SetAuthCookies(value); return Ok(value); },
                errors => Problem(errors));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var command = new LoginCommand(request.Email, request.Password);
            var result = await mediator.Send(command);

            return result.Match(
                value => { SetAuthCookies(value); return Ok(value); },
                errors => Problem(errors));
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            // Cookie (browser/WASM) takes priority, body as fallback (MAUI)
            var refreshToken = Request.Cookies["refresh_token"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                if (!Request.HasJsonContentType())
                    return BadRequest("Refresh token manquant");

                var body = await Request.ReadFromJsonAsync<RefreshTokenRequest>();
                refreshToken = body?.RefreshToken;
            }

            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest("Refresh token manquant");

            var command = new RefreshTokenCommand(refreshToken);
            var result = await mediator.Send(command);

            return result.Match(
                value => { SetAuthCookies(value); return Ok(value); },
                errors => Problem(errors));
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // Cookie (browser/WASM) takes priority, body as fallback (MAUI)
            var refreshToken = Request.Cookies["refresh_token"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                if (Request.HasJsonContentType())
                {
                    var body = await Request.ReadFromJsonAsync<LogoutRequest>();
                    refreshToken = body?.RefreshToken;
                }
            }

            ClearAuthCookies();

            if (string.IsNullOrEmpty(refreshToken))
                return NoContent();

            var command = new LogoutCommand(refreshToken);
            var result = await mediator.Send(command);

            return result.Match(
                _ => NoContent(),
                errors => Problem(errors));
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = GetUserId();

            var query = new GetCurrentUserQuery(userId);
            var result = await mediator.Send(query);

            return result.Match(
                value => Ok(value),
                errors => Problem(errors));
        }

        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateProfileRequest request)
        {
            var userId = GetUserId();

            var command = new UpdateUserCommand(userId, request.FirstName, request.LastName);
            var result = await mediator.Send(command);

            return result.Match(
                _ => NoContent(),
                errors => Problem(errors));
        }

        [HttpPost("me/photo")]
        [Authorize]
        [RequestFormLimits(MultipartBodyLengthLimit = 5 * 1024 * 1024 + 1024)]
        public async Task<IActionResult> UploadPhoto(IFormFile file)
        {
            var userId = GetUserId();

            var command = new UploadUserPhotoCommand(userId, file.OpenReadStream(), file.FileName, file.Length);
            var result = await mediator.Send(command);

            return result.Match(value => Ok(value), errors => Problem(errors));
        }

        [HttpDelete("me/photo")]
        [Authorize]
        public async Task<IActionResult> DeletePhoto()
        {
            var userId = GetUserId();

            var command = new DeleteUserPhotoCommand(userId);
            var result = await mediator.Send(command);

            return result.Match(value => Ok(value), errors => Problem(errors));
        }

        [HttpPut("me/password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = GetUserId();

            var command = new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword);
            var result = await mediator.Send(command);

            return result.Match(
                _ => NoContent(),
                errors => Problem(errors));
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var command = new ForgotPasswordCommand(request.Email);
            var result = await mediator.Send(command);

            return result.Match(
                _ => NoContent(),
                errors => Problem(errors));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var command = new ResetPasswordCommand(request.Email, request.Token, request.NewPassword);
            var result = await mediator.Send(command);

            return result.Match(
                _ => NoContent(),
                errors => Problem(errors));
        }

        private void SetAuthCookies(AuthResult result)
        {
            var secure = Request.IsHttps;
            var sameSite = secure ? SameSiteMode.None : SameSiteMode.Lax;

            Response.Cookies.Append("access_token", result.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = secure,
                SameSite = sameSite,
                Expires = result.ExpiresAt
            });

            Response.Cookies.Append("refresh_token", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = secure,
                SameSite = sameSite,
                Path = "/api/auth",
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });
        }

        private void ClearAuthCookies()
        {
            var secure = Request.IsHttps;
            var sameSite = secure ? SameSiteMode.None : SameSiteMode.Lax;

            Response.Cookies.Append("access_token", "", new CookieOptions
            {
                HttpOnly = true,
                Secure = secure,
                SameSite = sameSite,
                Expires = DateTimeOffset.UnixEpoch
            });

            Response.Cookies.Append("refresh_token", "", new CookieOptions
            {
                HttpOnly = true,
                Secure = secure,
                SameSite = sameSite,
                Path = "/api/auth",
                Expires = DateTimeOffset.UnixEpoch
            });
        }
    }
}
