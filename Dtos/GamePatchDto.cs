namespace Gameapi.Models;

public class GamePatchDto
{
    public string? Title { get; set; }
    public string? Developer { get; set; }
    public string? Genre { get; set; }
    public decimal? Price { get; set; }
    public int? ReleaseYear { get; set; }
    public double? Rating { get; set; }
    public int? Stock { get; set; }
}