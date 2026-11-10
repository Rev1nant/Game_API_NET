namespace Gameapi.Models;

public class User
{
    public int Id { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email {get; set; } = string.Empty;
    public int Balance { get; set; }
}