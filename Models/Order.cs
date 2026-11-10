namespace Gameapi.Models;

public class Order
{
    public int Id { get; set; }
    public int UserId {  get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public List<OrderItem> Items { get; set; } = new();
} 

public class OrderItem
{
    public int GameId { get; set; }
    public int Quantity { get; set; }
}