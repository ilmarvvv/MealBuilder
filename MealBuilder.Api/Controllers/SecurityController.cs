using MealBuilder.Api.Contracts.Security;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MealBuilder.Api.Controllers;

[ApiController]
[Route("api/security")]
public sealed class SecurityController(
    IAntiforgery antiforgery) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("csrf-token")]
    public ActionResult<AntiforgeryTokenResponse> GetCsrfToken()
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);

        var requestToken = tokens.RequestToken
            ?? throw new InvalidOperationException(
                "The antiforgery request token was not generated.");

        return Ok(new AntiforgeryTokenResponse(requestToken));
    }
}