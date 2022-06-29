using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SwaBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class HelloController : ControllerBase
{
    [HttpGet(nameof(Anonymous))]
    public string Anonymous()
    {
        return "Anyone can see this";
    }

    [HttpGet(nameof(Authenticated))]
    [Authorize(Roles = "authenticated", AuthenticationSchemes = "AzureStaticWebApps")]
    public string Authenticated()
    {
        return $"Only authenticated users can see this\n\n{JsonSerializer.Serialize(User.Identity)}";
    }
}
