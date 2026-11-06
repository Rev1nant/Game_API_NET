namespace Gameapi.Models;

public class UserPatchDto
{
    public string? Role { get; set; }
    public string? Name { get; set; }
    public string? Email {get; set; }
    public int? Amount { get; set; }
}