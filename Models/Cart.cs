namespace Gameapi.Models;

public class Cart
{
    public int UserId { get; set; }
    public int GameId { get; set; }
    public int Quantity { get; set; }
}


// public class Cart
// {
//     public int id { get; set; }
//     public List<CartGame> CartGames { get; set; } = new List<CartGame>();
// }

// public class CartGame
// {
//     public int GameId { get; set; }
//     public int Quantity { get; set; }
// }