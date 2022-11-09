using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace file_server.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly ILogger<FilesController> _logger;
    IConfiguration _configuration;
    public FilesController(ILogger<FilesController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

  

    [HttpGet("{container}")]
    public string GetFileList(string container)
    {
        _logger.LogInformation($"FileController::GetFiles for container {container} starts");
        string connectionString = _configuration.GetValue<string>("storagecs");
        BlobContainerClient blobClient = new BlobContainerClient(connectionString,container);
        if(blobClient.Exists())
        {
            Azure.Pageable<BlobItem> blobs = blobClient.GetBlobs();
            return GetBlobListAsJson(blobs, container);
        }
        else {
            _logger.LogError($"Container {container} provided does not exist");
            return $"Something is wrong, no files for {container}.";
        }
    }

    private string GetBlobListAsJson(Azure.Pageable<BlobItem> blobs, string containerName){
        dynamic result = new System.Dynamic.ExpandoObject();
        result.ContainerName = containerName;
        result.NumberOfBlobs = blobs.Count();
        BlobItem [] items = blobs.ToArray();
        result.Items = items;
        return JsonConvert.SerializeObject(result);
    }

}