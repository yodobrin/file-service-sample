using Microsoft.AspNetCore.Mvc;


namespace file_server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SaSTokenController : ControllerBase
{
    private readonly ILogger<SaSTokenController> _logger;
    IConfiguration _configuration;
 
    public SaSTokenController(ILogger<SaSTokenController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
 
    }

    [HttpGet(Name = "GetSaSToken")]
    public string GetSaSToken()
    {             
 
        // string secretValue = Conf
        string secretValue = _configuration.GetValue<string>("test-secret");
        string token = Guid.NewGuid().ToString(); 
        return $"this is a very super random token: {token} - {secretValue}";
    }



}

