using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace StreamShareAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class SpotifyController : ControllerBase
{
    // session information
    public const string SessionSpotifyStateKey = "_SpotifyState";
    public const string SessionUserIdKey = "_UserId";

    private readonly ILogger<SpotifyController> _logger;
    private readonly IConfiguration _configuration;

    public SpotifyController(ILogger<SpotifyController> logger, IConfiguration configuration)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IEnumerable<string>> HealthCheck()
    {
        return Ok("Spotify Controller is healthy");
    }

    [HttpGet("auth")]
    public IActionResult GetSpotifyLink()
    {
        // get userId to set in session
        int userId;
        var urlParams = HttpContext.Request.Query;
        if (urlParams.TryGetValue("userId", out StringValues userIdParam))
        {
            if (int.TryParse(userIdParam, out int parsedUserId))
            {
                userId = parsedUserId;
            }
            else
            {
                return BadRequest("UserId param is invalid.");
            }
        }
        else
        {
            return BadRequest("UserId param is missing.");
        }

        // get variables for request
        string baseUrl = "https://accounts.spotify.com/authorize";
        string redirectUri = _configuration["Spotify:RedirectUri"] ?? throw new InvalidOperationException("Spotify:RedirectUri is not configured.");
        string clientId = _configuration["Spotify:ClientId"] ?? throw new InvalidOperationException("Spotify:ClientId is not configured.");
        string state = Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..16];

        // query params
        var queryParams = new Dictionary<string, string>
        {
            { "state", state },
            { "scope", "user-read-private user-read-email" },
            { "response_type", "code" },
            { "redirect_uri", redirectUri},
            { "client_id", clientId }
        };

        // construct full url
        string spotifyAuthUrl = QueryHelpers.AddQueryString(baseUrl, queryParams!);

        // set important session info
        HttpContext.Session.SetString(SessionSpotifyStateKey, state);
        HttpContext.Session.SetInt32(SessionUserIdKey, userId);

        return Redirect(spotifyAuthUrl);
    }

    [HttpGet("redirect")]
    public IActionResult SpotifyAuthRedirect()
    {
        var queryParams = HttpContext.Request.Query;

        // check for state mismatch
        if (queryParams.TryGetValue("state", out StringValues spotifyState))
        {
            // get state from session
            string sessionState = HttpContext.Session.GetString(SessionSpotifyStateKey);

            // broken session
            if (sessionState == null)
            {
                return BadRequest("Session data is missing or expired.");
            }

            // check for state mismatch
            if (sessionState != spotifyState)
            {
                return StatusCode(401, "State Mismatch.");
            }
        }
        else
        {
            return BadRequest("State is missing from URL parameters.");
        }

        // check for any errors
        if (queryParams.TryGetValue("error", out StringValues errorMessage))
        {
            _logger.LogError("ERROR: An error occured authenticating with Spotify: {ErrorMessage}", errorMessage);

            // TODO redirect to error page upon unsuccessfull Spotify authentication
            return BadRequest(new { Error = "An error occured authenticating with Spotify. Redirect to error site."});
        }

        // update user profile


        return Ok();
    }
}

