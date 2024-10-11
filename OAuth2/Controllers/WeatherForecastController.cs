using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Polly;
using static OpenIddict.Abstractions.OpenIddictConstants;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace OAuth2.Controllers
{
    [ApiController]
    //[Route("[controller]")]
    [Route("/")]
    public class WeatherForecastController : Controller
    {
  
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        [Route("GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet]
        [Authorize]
        [Route("auth")]
        public IActionResult Test()
        {
            return Ok(new { message = "You have accessed a protected resource." });
        }

        [HttpGet]
        [HttpPost]
        [Route("connect/authorize")]
        [IgnoreAntiforgeryToken]
        public IResult Authorize()
        {
            // Display the login page.
            return Results.Ok("login page");
        }

        [HttpPost]
        [Route("connect/token")]
        public async Task<IResult> CreateToken()
        {
            try
            {
                //var httpContextAccessor = _httpContextAccessor.HttpContext;
                var request = _httpContextAccessor?.HttpContext?.Request ?? throw new InvalidOperationException("The request cannot be retrieved.");
                var clientId = request.Form["client_id"].FirstOrDefault();
                var clientSecret = request.Form["client_secret"].FirstOrDefault();
                var redirectUri = request.Form["redirect_uri"].FirstOrDefault();
                var scopes = request.Form["scope"].FirstOrDefault()?.Split(' ');
                var grantType = request.Form["grant_type"].FirstOrDefault();

                

                var applicationManager = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IOpenIddictApplicationManager>();

                if (applicationManager == null)
                {
                    return Results.Problem(detail: "The applicationManager cannot be retrieved.",  statusCode: 500);
                    //throw new InvalidOperationException("The applicationManager cannot be retrieved.");
                }

                if (grantType?.ToLower() == "client_credentials")
                {
                    var application = await applicationManager.FindByClientIdAsync(clientId)
                        ?? throw new InvalidOperationException("The application cannot be found.");

                    // Validate the client secret
                    if (!await applicationManager.ValidateClientSecretAsync(application, clientSecret))
                    {
                        throw new InvalidOperationException("Invalid client secret.");
                    }
                    
                    // Retrieve the user ID (in this case, the client ID)
                    var userId = await applicationManager.GetIdAsync(application);

                    // Create a new ClaimsIdentity containing the claims
                    var identity = new ClaimsIdentity(
                        TokenValidationParameters.DefaultAuthenticationType,
                        //OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        OpenIddictConstants.Claims.Name,
                        OpenIddictConstants.Claims.Role);

                    // Add necessary claims
                    identity.AddClaim(Claims.Subject, userId, OpenIddictConstants.Destinations.AccessToken);
                    identity.AddClaim("client_id", clientId ?? string.Empty, OpenIddictConstants.Destinations.AccessToken);

                    var principal = new ClaimsPrincipal(identity);
                    //var scopes = request.GetScopes();
                    principal.SetScopes(scopes);

                    foreach (var claim in principal.Claims)
                    {
                        claim.SetDestinations(OpenIddictConstants.Destinations.AccessToken);
                    }

                    // Create a new authentication ticket holding the identity
                    var ticket = new AuthenticationTicket(
                        principal,
                        new AuthenticationProperties(),
                        OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                    
                    // Sign in and return the token
                    var signin = Results.SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
                    //return Results.Ok(signin);
                    return signin;

                    //return tokenRequestContext.SignIn(principal);
                }
                else
                {
                    return Results.Ok(grantType?.ToLower());
                }
            }
            catch (Exception ex)
            {
                //return BadRequest(new { message = ex.Message }) as IActionResult;
                return Results.Problem(detail: ex.Message, statusCode: 500);
            }

            return Results.Problem(detail: "No Idea", statusCode: 500);
        }
    }
}
