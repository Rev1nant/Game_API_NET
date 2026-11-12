namespace Gameapi.Models;

public class Order
{
    public int Id { get; set; }
    public int UserId {  get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
} 
