using Microsoft.AspNetCore.Mvc;

namespace file_server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly ILogger<FilesController> _logger;

    public FilesController(ILogger<FilesController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public string GetFileList()
    {
        string token = Guid.NewGuid().ToString(); 
        return $"this is a list of files: {token}";
    }

    [HttpGet("{id}")]
    public string GetFile(string id)
    {
        string token = Guid.NewGuid().ToString(); 
        return $"this is a random token: {token} for the {id} provided.";
    }

}