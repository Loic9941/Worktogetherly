using WorkTogetherly.Application.Users.GetCurrentUser;
using WorkTogetherly.Domain.Entities;

namespace WorkTogetherly.Application.Users.Common;

public static class UserMapper
{
    public static CurrentUserResult ToResult(this User user) =>
        new(user.Id, user.Email!, user.FirstName, user.LastName, user.PhotoPath);
}
