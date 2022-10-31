using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web.Resource;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Newtonsoft.Json;
using System.Security.Claims;

namespace file_server.Controllers;

[Authorize(Policy = "NamedPeopleOnly")]
[RequiredScopeOrAppPermission(AcceptedScope = new string[] { "User.Read" })]
[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    // private const string _userReadScope = "User.Read";
    private readonly IHttpContextAccessor _contextAccessor;

    private readonly ILogger<FilesController> _logger;
    IConfiguration _configuration;
    public FilesController(ILogger<FilesController> logger, IConfiguration configuration, IHttpContextAccessor contextAccessor)
    {
        _logger = logger;
        _configuration = configuration;
        _contextAccessor = contextAccessor;
    }

    /// returns the current claimsPrincipal (user/Client app) dehydrated from the Access token
    private ClaimsPrincipal? GetCurrentClaimsPrincipal()
    {
        if (_contextAccessor.HttpContext != null && _contextAccessor.HttpContext.User != null)
        {
            return _contextAccessor.HttpContext.User;
        }
        return null;
    }

    private bool IsAppOnlyToken()
    {
        // Add in the optional 'idtyp' claim to check if the access token is coming from an application or user.
        //
        // See: https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-optional-claims
        ClaimsPrincipal? claimsPrincipal = GetCurrentClaimsPrincipal();
        if (claimsPrincipal != null)
        {
            return claimsPrincipal.Claims.Any(c => c.Type == "idtyp" && c.Value == "app");
        }
        return false;
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