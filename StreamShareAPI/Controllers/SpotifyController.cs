using Microsoft.AspNetCore.Mvc;

namespace StreamShareAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class SpotifyController : ControllerBase
{
    private readonly ILogger<SpotifyController> _logger;

    public SpotifyController(ILogger<SpotifyController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IEnumerable<string>> Get()
    {
        return Ok("Spotify Controller");
    }
}

