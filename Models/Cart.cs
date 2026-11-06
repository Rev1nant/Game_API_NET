namespace Gameapi.Models;

public class Cart
{
    public int UserId { get; set; }
    public int GameId { get; set; }
    public int Quantity { get; set; }
}