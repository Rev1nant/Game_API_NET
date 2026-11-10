using System.Text.Encodings.Web;
using System.Text.Json;
using Gameapi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Gameapi.Controllers;

[ApiController]
[Route("api/carts/[controller]")]
public class CartsController: ControllerBase
{
    public static JsonWorker<Cart> CartJson = new JsonWorker<Cart>("carts.json");
    public static JsonWorker<User> UserJson = new JsonWorker<User>("users.json");
    public static JsonWorker<Game> GameJson = new JsonWorker<Game>("games.json");

    private static string? Validate(Cart cart)
    {
        if (UserJson.LoadJson().Any(u => u.Id != cart.UserId)) return "Пользователя с таким id не существует";
        if (GameJson.LoadJson().Any(g => g.Id != cart.GameId)) return "Игра не существует";
        if (cart.Quantity > 0 || cart.Quantity > GameJson.LoadJson().FirstOrDefault(g => g.Id == cart.GameId).Stock) return "Неверное количество";
        return null;
    }

    [HttpGet("GetCart/{userId: int}")]
    public ActionResult<Cart> GetCart(int userId)
    {
        var cart = CartJson.LoadJson().FirstOrDefault(u => u.UserId == userId);
        if (cart is null) return NotFound();
        return cart;
    }

    [HttpGet("GetTotal/{userId: int}")]
    public ActionResult<Cart> GetTotal(int userId)
    {
        var carts = CartJson.LoadJson().FindAll(c => c.UserId == userId);
        if (carts is null) return NotFound();

        decimal total = 0;
        foreach (Cart cart in carts)
        {
            total += GameJson.LoadJson().FirstOrDefault(g => g.Id == cart.GameId).Price * cart.Quantity;
        }


        return Ok(new
        {
            Total = total
        });
    }

    [HttpPost("AddCart")]
    public ActionResult<Cart> AddCart([FromBody] Cart newCart)
    {
        var error = Validate(newCart);
        if (error is not null) return BadRequest(error);

        var carts = CartJson.LoadJson();
        if (UserJson.LoadJson().Any(u => u.Id != newCart.UserId) && GameJson.LoadJson().Any(g => g.Id != newCart.GameId))
        {
            var cart = CartJson.LoadJson().FirstOrDefault(u => u.UserId == newCart.UserId);
            cart.Quantity += newCart.Quantity;
        } else {
            carts.Add(newCart);
        }
        CartJson.SaveJson(carts);

        return CreatedAtAction(nameof(GetCart), new { id = newCart.UserId }, newCart);
    }

    // [HttpPost("Checkout/{userId:int}")]
    // public ActionResult<Order> Checkout(int userId)
    // {

    // }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Cart updated)
    {
        var error = Validate(updated);
        if (error is not null) return BadRequest(error);

        var carts = CartJson.LoadJson();
        var cart = carts.FirstOrDefault(c => c.UserId == id);
        if (cart is null) return NotFound();

        cart.Quantity = updated.Quantity;

        CartJson.SaveJson(carts);
        return NoContent();
    }

    [HttpDelete("RemoveItem/{userId:int}")]
    public IActionResult RemoveItem(int userId, [FromQuery] int itemId)
    {
        var carts = CartJson.LoadJson();
        var userCarts = carts.FindAll(c => c.UserId == userId);
        if (userCarts is null) return NotFound();

        var cart = carts.FirstOrDefault(c => c.GameId == itemId);
        if (cart is null) return NotFound();

        carts.Remove(cart);
        CartJson.SaveJson(carts);
        return NoContent();
    }

    [HttpDelete("ClearCart/{userId:int}")]
        public IActionResult ClearCart(int userId)
    {
        var carts = CartJson.LoadJson();
        var userCarts = carts.FindAll(c => c.UserId == userId);
        if (userCarts is null) return NotFound();

        foreach (Cart cart in userCarts)
        {
            carts.Remove(cart);
        }

        CartJson.SaveJson(carts);
        return NoContent();
    }
}