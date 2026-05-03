namespace ControleCerto.DTOs.User;

using ControleCerto.Enums;

public class DetailsUserResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsAdmin { get; set; }
    public UserTypeEnum UserType { get; set; }
    public DateTime CreatedAt { get; set; }
}
