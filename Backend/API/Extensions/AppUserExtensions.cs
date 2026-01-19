using API.DTOs;
using API.Entities;

public static class AppUserExtensions
{
    public static UserDTO ToUserDTO(this User user, string token)
    {
        return new UserDTO
        {
            Id = user.UserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Token = token,
        };
    }
}