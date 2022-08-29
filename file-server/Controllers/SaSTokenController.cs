using Microsoft.AspNetCore.Mvc;

namespace file_server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SaSTokenController : ControllerBase
{
    private readonly ILogger<SaSTokenController> _logger;

    public SaSTokenController(ILogger<SaSTokenController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetSaSToken")]
    public string GetSaSToken()
    {
        string token = Guid.NewGuid().ToString(); 
        return $"this is a random token: {token}";
    }



}