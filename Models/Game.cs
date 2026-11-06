namespace Gameapi.Models;

public class Game
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Developer { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int ReleaseYear { get; set; }
    public double Rating { get; set; }
    public int Stock { get; set; }
}