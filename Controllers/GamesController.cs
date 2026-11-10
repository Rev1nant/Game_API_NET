using System.Text.Encodings.Web;
using System.Text.Json;
using Gameapi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

namespace Gameapi.Controllers;

[ApiController]
[Route("api/games/[controller]")]
public class GamesController: ControllerBase
{
    public static JsonWorker<Game> GamesJson = new JsonWorker<Game>("games.json");

    private static string? Validate(Game game)
    {
        if (string.IsNullOrWhiteSpace(game.Title)) return "Название игры обязательно.";
        if (game.Price < 0) return "Цена не может быть отрицательной.";
        if (game.Rating < 0 || game.Rating > 10) return "Рейтинг должен быть от 0 до 10";
        if (game.Stock < 0) return "Остаток на складе не может быть отрицательным";
        return null;
    }

    [HttpGet("GetAll")]
    public ActionResult<List<Game>> GetAll([FromQuery] string? genre, [FromQuery] decimal? maxPrice)
    {
        var games = GamesJson.LoadJson().AsEnumerable();

        if (!string.IsNullOrWhiteSpace(genre))
            games = games.Where(g => g.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase));
        if (maxPrice.HasValue)
            games = games.Where(g => g.Price <= maxPrice.Value);

        return games.ToList();
    }

    [HttpGet("GetById/{id:int}")]
    public ActionResult<Game> GetById(int id)
    {
        var game = GamesJson.LoadJson().FirstOrDefault(g => g.Id == id);
        if (game is null) return NotFound();
        return game;
    }

    [HttpGet("GetTopRated")]
    public ActionResult<List<Game>> GetTopRated([FromQuery] double minRating = 8)
    {
        return GamesJson.LoadJson()
            .Where(g => g.Rating >= minRating)
            .OrderByDescending(g => g.Rating)
            .ToList();
    }

    [HttpGet("GetStats")]
    public IActionResult GetStats()
    {
        var games = GamesJson.LoadJson();
        if (games.Count == 0)
            return Ok(new { totalGames = 0 });

        return Ok(new
        {
            totalGames = games.Count,
            averagePrice = Math.Round(games.Average(g => g.Price), 2),
            averageRating = Math.Round(games.Average(g => g.Rating), 2),
            mostExpensive = games.MaxBy(g => g.Price),
            cheapest = games.MinBy(g => g.Price),
            totalStockValue = games.Sum(g => g.Price * g. Stock),
            gamesByGenre = games
                .GroupBy(g => g.Genre)
                .Select(gr => new { genre = gr.Key, count = gr.Count() })
        });
    }

    [HttpPost("AddGame")]
    public ActionResult<Game> AddGame([FromBody] Game newGame)
    {
        var error = Validate(newGame);
        if (error is not null) return BadRequest(error);

        var games = GamesJson.LoadJson();
        newGame.Id = games.Count > 0 ? games.Max(g => g.Id) + 1 : 1;
        games.Add(newGame);
        GamesJson.SaveJson(games);

        return CreatedAtAction(nameof(GetById), new { id = newGame.Id }, newGame);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Game updated)
    {
        var error = Validate(updated);
        if (error is not null) return BadRequest(error);

        var games = GamesJson.LoadJson();
        var game = games.FirstOrDefault(g => g.Id == id);
        if (game is null) return NotFound();

        game.Title = updated.Title;
        game.Developer = updated.Developer;
        game.Genre = updated.Genre;
        game.Price = updated.Price;
        game.ReleaseYear = updated.ReleaseYear;
        game.Rating = updated.Rating;
        game.Stock = updated.Stock;

        GamesJson.SaveJson(games);
        return NoContent();
    }

    [HttpPatch("Patch/{id:int}")]
    public ActionResult<Game> Patch(int id, [FromBody] GamePatchDto patch)
    {
        var games = GamesJson.LoadJson();
        var game = games.FirstOrDefault(g => g.Id == id);
        if (game is null) return NotFound();

        if (patch.Title is not null) game.Title = patch.Title;
        if (patch.Developer is not null) game.Developer = patch.Developer;
        if (patch.Genre is not null) game.Genre = patch.Genre;
        if (patch.Price.HasValue) game.Price = patch.Price.Value;
        if (patch.ReleaseYear.HasValue) game.ReleaseYear = patch.ReleaseYear.Value;
        if (patch.Rating.HasValue) game.Rating = patch.Rating.Value;
        if (patch.Stock.HasValue) game.Stock = patch.Stock.Value;

        var error = Validate(game);
        if (error is not null) return BadRequest(error);

        GamesJson.SaveJson(games);
        return game;
    }

    [HttpDelete("Delete/{id:int}")]
    public IActionResult Delete(int id)
    {
        var games = GamesJson.LoadJson();
        var game = games.FirstOrDefault(g => g.Id == id);
        if (game is null) return NotFound();

        games.Remove(game);
        GamesJson.SaveJson(games);
        return NoContent();
    }
}