using System;
using System.Collections.Generic;
using System.Text;

namespace WorkTogetherly.Shared.Models
{
    public record AuthResult(string AccessToken, string RefreshToken, DateTime ExpiresAt);
}
