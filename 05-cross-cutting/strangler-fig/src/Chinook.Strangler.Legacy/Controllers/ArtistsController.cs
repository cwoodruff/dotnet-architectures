using System.Data;
using System.Text.Json;
using Chinook.Strangler.Legacy.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Chinook.Strangler.Legacy.Controllers;

// Deliberately written the way many systems being replaced are: the controller opens the
// connection, calls the stored procedure, and parses the result itself. There is no repository,
// no service layer, and no abstraction to hide behind. The Facade is what lets this keep running
// untouched while Modern takes its routes one at a time.
public class ArtistsController(IConfiguration configuration) : Controller
{
    public async Task<IActionResult> Index()
    {
        var artists = new List<ArtistRecord>();

        using (var connection = new SqlConnection(configuration.GetConnectionString("Chinook")))
        using (var command = new SqlCommand("sproc_GetArtist", connection))
        {
            command.CommandType = CommandType.StoredProcedure;
            await connection.OpenAsync();

            // The Chinook Get procedures return one row with a JSONDATA column holding a JSON array.
            var json = await command.ExecuteScalarAsync() as string;
            if (!string.IsNullOrEmpty(json))
            {
                artists = JsonSerializer.Deserialize<List<ArtistRecord>>(json) ?? new List<ArtistRecord>();
            }
        }

        artists = artists.OrderBy(a => a.Name).ToList();
        return View(artists);
    }
}
