namespace TechNova.ViewModels;

public sealed class UserManagementViewModel
{
    public string UserId { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public int OrderCount { get; init; }
}
