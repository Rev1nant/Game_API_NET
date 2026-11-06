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
        var cart = CartJson.LoadJson().FirstOrDefault(c => c.UserId == userId);
        if (cart is null) return NotFound();
        return Ok(new
        {
            Total = cart // Дописать
        });
    }

    [HttpPost("AddCart")]
    public ActionResult<Cart> AddCart([FromBody] Cart newCart)
    {
        var error = Validate(newCart);
        if (error is not null) return BadRequest(error);

        var carts = CartJson.LoadJson();
        if (UserJson.LoadJson().Any(u => u.Id != newCart.UserId) && GameJson.LoadJson().Any(g => g.Id != newCart.GameId) {
            // Дописать
        } else {
            carts.Add(newCart);
        }
        CartJson.SaveJson(carts);

        return CreatedAtAction(nameof(GetCart), new { id = newCart.UserId }, newCart);
    }

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

    [HttpDelete("Delete/{id:int}")]
    public IActionResult Delete(int id)
    {
        var carts = CartJson.LoadJson();
        var cart = carts.FirstOrDefault(g => g.Id == id);
        if (cart is null) return NotFound();

        carts.Remove(cart);
        CartJson.SaveJson(carts);
        return NoContent();
    }

}