using System;
using System.Collections.Generic;
using System.Text;

namespace WorkTogetherly.Application.Users.Shared
{
    public record AuthResult(
        string AccessToken,
        string RefreshToken,
        DateTime ExpiresAt
    );
}
