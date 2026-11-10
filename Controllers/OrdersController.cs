using System.Text.Encodings.Web;
using System.Text.Json;
using Gameapi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Gameapi.Controllers;

[ApiController]
[Route("api/orders/[controller]")]
public class OrdersController: ControllerBase
{
    public static JsonWorker<Order> OrderJson = new JsonWorker<Order>("orders.json");

    // private static string? Validate(Order order)
    // {
    //     var orders = OrderJson.LoadJson();

    //     if (string.IsNullOrWhiteSpace(order.Name)) return "Имя обязательно.";
    //     if (string.IsNullOrWhiteSpace(order.Email)) return "email обязательно.";
    //     if (orders.Any(u => u.Email == order.Email)) return "email уже зарегистрирован";
    //     if (order.Balance < 0) return "Баланс не может быть отрицательным";
    //     return null;
    // }

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

    // [HttpPost("AddOrder")]
    // public ActionResult<Order> AddOrder([FromBody] Order newOrder)
    // {
    //     var error = Validate(newOrder);
    //     if (error is not null) return BadRequest(error);

    //     var orders = OrderJson.LoadJson();
    //     newOrder.Id = orders.Count > 0 ? orders.Max(g => g.Id) + 1 : 1;
    //     orders.Add(newOrder);
    //     OrderJson.SaveJson(orders);

    //     return CreatedAtAction(nameof(GetById), new { id = newOrder.Id }, newOrder);
    // }

    // [HttpPost("{id:int}/TopUp")]
    // public ActionResult<Order> TopUp(int id, [FromQuery] int amount)
    // {
    //     if (amount <= 0) return BadRequest("Неверная сумма пополнения");

    //     var orders = OrderJson.LoadJson();
    //     var order = orders.FirstOrDefault(g => g.Id == id);
    //     if (order is null) return NotFound();

    //     order.Balance += amount;

    //     OrderJson.SaveJson(orders);
    //     return order;
    // }

    // [HttpPut("{id:int}")]
    // public IActionResult Update(int id, [FromBody] Order updated)
    // {
    //     var error = Validate(updated);
    //     if (error is not null) return BadRequest(error);

    //     var orders = OrderJson.LoadJson();
    //     var order = orders.FirstOrDefault(g => g.Id == id);
    //     if (order is null) return NotFound();

    //     order.Role = updated.Role;
    //     order.Name = updated.Name;
    //     order.Email = updated.Email;
    //     order.Balance = updated.Balance;

    //     OrderJson.SaveJson(orders);
    //     return NoContent();
    // }

    // [HttpPatch("Patch/{id:int}")]
    // public ActionResult<Order> Patch(int id, [FromBody] OrderPatchDto patch)
    // {
    //     var orders = OrderJson.LoadJson();
    //     var order = orders.FirstOrDefault(g => g.Id == id);
    //     if (order is null) return NotFound();

    //     if (patch.Role is not null) order.Role = patch.Role;
    //     if (patch.Name is not null) order.Name = patch.Name;
    //     if (patch.Email is not null) order.Email = patch.Email;
    //     if (patch.Amount.HasValue) order.Balance = patch.Amount.Value;

    //     var error = Validate(order);
    //     if (error is not null) return BadRequest(error);

    //     OrderJson.SaveJson(orders);
    //     return order;
    // }

    // [HttpDelete("Delete/{id:int}")]
    // public IActionResult Delete(int id)
    // {
    //     var orders = OrderJson.LoadJson();
    //     var order = orders.FirstOrDefault(g => g.Id == id);
    //     if (order is null) return NotFound();

    //     orders.Remove(order);
    //     OrderJson.SaveJson(orders);
    //     return NoContent();
    // }

}