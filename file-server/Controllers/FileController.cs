using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Newtonsoft.Json;

namespace file_server.Controllers;

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

    // [HttpGet]
    // // public string GetFileList()
    // // {
    // //     string token = Guid.NewGuid().ToString(); 
    // //     return $"this is a list of files: {token}";
    // // }

    [HttpGet("{id}")]
    public string GetFile(string id)
    {
        string token = Guid.NewGuid().ToString(); 
        string connectionString = _configuration.GetValue<string>("storagecs");
        BlobContainerClient blobClient = new BlobContainerClient(connectionString,id);
        if(blobClient.Exists())
        {
            Azure.Pageable<BlobItem> blobs = blobClient.GetBlobs();
            return GetBlobListAsJson(blobs, id);
        }
        else return $"Something is wrong, no files for {id}.";
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