using System.Data;
using System.Text.Json;
using Chinook.Strangler.Legacy.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Chinook.Strangler.Legacy.Controllers;

public class AlbumsController(IConfiguration configuration) : Controller
{
    public async Task<IActionResult> Index(int artistId)
    {
        var model = new AlbumsViewModel { ArtistId = artistId };

        using (var connection = new SqlConnection(configuration.GetConnectionString("Chinook")))
        using (var command = new SqlCommand("sproc_GetAlbumByArtist", connection))
        {
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@ArtistId", SqlDbType.Int).Value = artistId;
            await connection.OpenAsync();

            var json = await command.ExecuteScalarAsync() as string;
            if (!string.IsNullOrEmpty(json))
            {
                model.Albums = JsonSerializer.Deserialize<List<AlbumRecord>>(json) ?? new List<AlbumRecord>();
            }
        }

        return View(model);
    }
}
