using System.Text.Encodings.Web;
using System.Text.Json;
using Gameapi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Gameapi.Controllers;

[ApiController]
[Route("api/users/[controller]")]
public class UsersController: ControllerBase
{
    public static JsonWorker<User> UserJson = new JsonWorker<User>("users.json");

    private static string? Validate(User user)
    {
        var users = UserJson.LoadJson();

        if (string.IsNullOrWhiteSpace(user.Name)) return "Имя обязательно.";
        if (string.IsNullOrWhiteSpace(user.Email)) return "email обязательно.";
        if (users.Any(u => u.Email == user.Email)) return "email уже зарегистрирован";
        if (user.Balance < 0) return "Баланс не может быть отрицательным";
        return null;
    }

    [HttpGet("GetAll")]
    public ActionResult<List<User>> GetAll([FromQuery] string? role, [FromQuery] string? name)
    {
        var users = UserJson.LoadJson().AsEnumerable();

        if (!string.IsNullOrWhiteSpace(role))
            users = users.Where(u => u.Role.Equals(role, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(name))
            users = users.Where(u => u.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        return users.ToList();
    }

    [HttpGet("GetById/{id:int}")]
    public ActionResult<User> GetById(int id)
    {
        var user = UserJson.LoadJson().FirstOrDefault(u => u.Id == id);
        if (user is null) return NotFound();
        return user;
    }

    [HttpGet("GetByEmail")]
    public ActionResult<List<User>> GetByEmail([FromQuery] string email)
    {
        var users = UserJson.LoadJson().FindAll(u => u.Email == email);
        if (users is null) return NotFound();
        return users;
    }

    // [HttpGet("GetStats")]
    // public IActionResult GetStats()
    // {
    //     var users = UsersJson.LoadJson();
    //     if (users.Count == 0)
    //         return Ok(new { totalUsers = 0 });

    //     return Ok(new
    //     {
    //         totalUsers = users.Count,
    //         averagePrice = Math.Round(users.Average(g => g.Price), 2),
    //         averageRating = Math.Round(users.Average(g => g.Rating), 2),
    //         mostExpensive = users.MaxBy(g => g.Price),
    //         cheapest = users.MinBy(g => g.Price),
    //         totalStockValue = users.Sum(g => g.Price * g. Stock),
    //         usersByGenre = users
    //             .GroupBy(g => g.Genre)
    //             .Select(gr => new { genre = gr.Key, count = gr.Count() })
    //     });
    // }

    [HttpPost("AddUser")]
    public ActionResult<User> AddUser([FromBody] User newUser)
    {
        var error = Validate(newUser);
        if (error is not null) return BadRequest(error);

        var users = UserJson.LoadJson();
        newUser.Id = users.Count > 0 ? users.Max(g => g.Id) + 1 : 1;
        users.Add(newUser);
        UserJson.SaveJson(users);

        return CreatedAtAction(nameof(GetById), new { id = newUser.Id }, newUser);
    }

    [HttpPost("{id:int}/TopUp")]
    public ActionResult<User> TopUp(int id, [FromQuery] int amount)
    {
        if (amount <= 0) return BadRequest("Неверная сумма пополнения");

        var users = UserJson.LoadJson();
        var user = users.FirstOrDefault(g => g.Id == id);
        if (user is null) return NotFound();

        user.Balance += amount;

        UserJson.SaveJson(users);
        return user;
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] User updated)
    {
        var error = Validate(updated);
        if (error is not null) return BadRequest(error);

        var users = UserJson.LoadJson();
        var user = users.FirstOrDefault(g => g.Id == id);
        if (user is null) return NotFound();

        user.Role = updated.Role;
        user.Name = updated.Name;
        user.Email = updated.Email;
        user.Balance = updated.Balance;

        UserJson.SaveJson(users);
        return NoContent();
    }

    [HttpPatch("Patch/{id:int}")]
    public ActionResult<User> Patch(int id, [FromBody] UserPatchDto patch)
    {
        var users = UserJson.LoadJson();
        var user = users.FirstOrDefault(g => g.Id == id);
        if (user is null) return NotFound();

        if (patch.Role is not null) user.Role = patch.Role;
        if (patch.Name is not null) user.Name = patch.Name;
        if (patch.Email is not null) user.Email = patch.Email;
        if (patch.Amount.HasValue) user.Balance = patch.Amount.Value;

        var error = Validate(user);
        if (error is not null) return BadRequest(error);

        UserJson.SaveJson(users);
        return user;
    }

    [HttpDelete("Delete/{id:int}")]
    public IActionResult Delete(int id)
    {
        var users = UserJson.LoadJson();
        var user = users.FirstOrDefault(g => g.Id == id);
        if (user is null) return NotFound();

        users.Remove(user);
        UserJson.SaveJson(users);
        return NoContent();
    }

}