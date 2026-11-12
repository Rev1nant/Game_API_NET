using System.Text.Encodings.Web;
using System.Text.Json;
using Gameapi.Models;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Gameapi.Controllers;

[ApiController]
[Route("api/orders/[controller]")]
public class OrdersController: ControllerBase
{
    public static JsonWorker<Order> OrderJson = new JsonWorker<Order>("orders.json");
    public static JsonWorker<Cart> CartJson = new JsonWorker<Cart>("carts.json");
    public static JsonWorker<User> UserJson = new JsonWorker<User>("users.json");
    public static JsonWorker<Game> GameJson = new JsonWorker<Game>("games.json");
    private static string? Validate(Order order)
    {
        var orders = OrderJson.LoadJson();
        var users = UserJson.LoadJson();
        var games = GameJson.LoadJson();
        var carts = CartJson.LoadJson();

        var user = users.FirstOrDefault(u => u.Id == order.UserId);
        if (user is null) return "Пользователя с таким id не существует";

        // foreach(int item in order.Items)
        // {
        //     var cart = carts.FirstOrDefault(c => c.UserId == item);
        //     var game = games.FirstOrDefault(g => g.Id == item);
        //     if (game is null) return "Игры не существует";
        //     if (!(item.Quantity > 0 && item.Quantity <= game.Stock)) return "Количество не может быть меньше 0 или больше stock";
        // }
        return null;
    }

    [HttpGet("GetAll")]
    public ActionResult<List<Order>> GetAll([FromQuery] int? userId, [FromQuery] string? status, [FromQuery] decimal? minTotal)
    {
        var orders = OrderJson.LoadJson().AsEnumerable();

        if (!string.IsNullOrWhiteSpace(status))
            orders = orders.Where(o => o.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        if (userId.HasValue)
            orders = orders.Where(o => o.UserId == userId.Value);
        if (minTotal.HasValue)
            orders = orders.Where(o => o.TotalAmount >= minTotal.Value);

        return orders.ToList();
    }

    [HttpGet("GetById/{id:int}")]
    public ActionResult<Order> GetById(int id)
    {
        var order = OrderJson.LoadJson().FirstOrDefault(o => o.Id == id);
        if (order is null) return NotFound();
        return order;
    }

    [HttpGet("GetByUser/{userId:int}")]
    public ActionResult<List<Order>> GetByUser(int userId)
    {
        var orders = OrderJson.LoadJson().FindAll(o => o.UserId == userId);
        if (orders is null) return NotFound();
        return orders;
    }


    // // [HttpGet("GetStats")]
    // // public IActionResult GetStats()
    // // {
    // //     var orders = OrdersJson.LoadJson();
    // //     if (orders.Count == 0)
    // //         return Ok(new { totalOrders = 0 });

    // //     return Ok(new
    // //     {
    // //         totalOrders = orders.Count,
    // //         averagePrice = Math.Round(orders.Average(g => g.Price), 2),
    // //         averageRating = Math.Round(orders.Average(g => g.Rating), 2),
    // //         mostExpensive = orders.MaxBy(g => g.Price),
    // //         cheapest = orders.MinBy(g => g.Price),
    // //         totalStockValue = orders.Sum(g => g.Price * g. Stock),
    // //         ordersByGenre = orders
    // //             .GroupBy(g => g.Genre)
    // //             .Select(gr => new { genre = gr.Key, count = gr.Count() })
    // //     });
    // // }

    [HttpPost("AddOrder")]
    public ActionResult<Order> AddOrder([FromBody] Order newOrder)
    {
        var games = GameJson.LoadJson();
        var carts = CartJson.LoadJson();

        var error = Validate(newOrder);
        if (error is not null) return BadRequest(error);

        var orders = OrderJson.LoadJson();
        newOrder.Id = orders.Count > 0 ? orders.Max(g => g.Id) + 1 : 1;

        var userCarts = carts.FindAll(c => c.UserId == newOrder.UserId);
        if (userCarts is null) return BadRequest("Корзина не должен быть пустым");
        decimal sumTotal = 0;
        foreach (Cart cart in userCarts)
        {
            var game = games.FirstOrDefault(g => g.Id == cart.GameId);
            if (game is null) return BadRequest("Игры не существует");
            sumTotal += cart.Quantity * game.Price;
        }
        newOrder.TotalAmount = sumTotal;
        orders.Add(newOrder);
        OrderJson.SaveJson(orders);

        return CreatedAtAction(nameof(GetById), new { id = newOrder.Id }, newOrder);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Order updated)
    {
        var error = Validate(updated);
        if (error is not null) return BadRequest(error);

        var orders = OrderJson.LoadJson();
        var order = orders.FirstOrDefault(g => g.Id == id);
        if (order is null) return NotFound();
        if (order.Status != "New") return BadRequest("Заказ нельзя обновить");

        order.UserId = updated.UserId;
        order.Status = updated.Status;
        order.TotalAmount = updated.TotalAmount;

        OrderJson.SaveJson(orders);
        return NoContent();
    }

    [HttpPatch("Patch/{id:int}")]
    public ActionResult<Order> Patch(int id, [FromBody] string newStatus)
    {
        var orders = OrderJson.LoadJson();
        var order = orders.FirstOrDefault(o => o.Id == id);
        if (order is null) return NotFound();

        if (newStatus != "New" || newStatus != "Paid" || newStatus != "Shipped" || newStatus != "Cancelled") return BadRequest("Такого статуса не существует");
        if (newStatus is not null) order.Status = newStatus;

        OrderJson.SaveJson(orders);
        return order;
    }

    [HttpDelete("Cancel/{id:int}")]
    public IActionResult Cancel(int id)
    {
        var orders = OrderJson.LoadJson();
        var order = orders.FirstOrDefault(g => g.Id == id);
        if (order is null) return NotFound();

        orders.Remove(order);
        OrderJson.SaveJson(orders);
        return NoContent();
    }

}