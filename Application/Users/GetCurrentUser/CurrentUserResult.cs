using System;
using System.Collections.Generic;
using System.Text;

namespace WorkTogetherly.Application.Users.GetCurrentUser
{
    public record CurrentUserResult(
        Guid Id,
        string Email,
        string FirstName,
        string LastName,
        string? PhotoPath = null
    );
}
