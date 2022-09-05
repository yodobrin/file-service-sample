using Microsoft.AspNetCore.Mvc;



using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Azure.Storage.Blobs.Models;
using System.Net;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http.Features;

namespace file_server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SaSTokenController : ControllerBase
{
    private readonly ILogger<SaSTokenController> _logger;
    IConfiguration _configuration;
    
    private readonly static string XENVOY_IP = "X-Envoy-External-Address";
    private readonly static string XFWD4_IP = "x-forwarded-for";
    private readonly static string DEFAULT_IP = "1.1.1.1";
    public SaSTokenController(ILogger<SaSTokenController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
 
    }

    [HttpGet(Name = "GetSaSToken")]
    public string GetSaSToken()
    {                                                        
        // in case no container name is passed as part of the request, generate a new name
        string containerName = Guid.NewGuid().ToString();
        return CreateSasToken(containerName);
        // return $"Container: {containerName} |Period - {_configuration["access_period"]} - ip {remoteIp.MapToIPv6()} |";
    }

    [HttpGet("{containerName}")]
    public string GetSaSToken(string containerName)
    {
        return CreateSasToken(containerName);
    }

    private string CreateSasToken(string containerName)
    {
        dynamic result = new System.Dynamic.ExpandoObject();
        result.ContainerName = containerName;
        var remoteIp = IPAddress.Parse(FindRemoteIp());
        result.RemoteIp = remoteIp.ToString();
        // need to check that it is not 1.1.1.1
        string connectionString = _configuration.GetValue<string>("storagecs");
        BlobContainerClient blobClient = new BlobContainerClient(connectionString,containerName);
        blobClient.CreateIfNotExists();
        Uri sas = GetServiceSasUriForContainer(blobClient, result.RemoteIp);
        result.SasTokenUri = sas.AbsoluteUri;
        result.SasTokenPath = sas.AbsolutePath;
        return JsonConvert.SerializeObject(result);
        // return string.Empty;
    }
    

    private Uri GetServiceSasUriForContainer(BlobContainerClient containerClient, string remoteIp )
    {
        // Check whether this BlobContainerClient object has been authorized with Shared Key.
        if (containerClient.CanGenerateSasUri)
        {
            // Create a SAS token that's valid for one hour.
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = containerClient.Name,
                Resource = "b"
            };

            sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(double.Parse(_configuration["access_period_minutes"]));
            // sasBuilder.SetPermissions(BlobContainerSasPermissions.All);
            sasBuilder.SetPermissions(BlobSasPermissions.Add);
            sasBuilder.SetPermissions(BlobSasPermissions.Create);
            sasBuilder.SetPermissions(BlobSasPermissions.List);
            // sasBuilder.SetPermissions(BlobContainerSasPermissions.Create);
            // sasBuilder.SetPermissions(BlobContainerSasPermissions.Add);
            // sasBuilder.SetPermissions(BlobContainerSasPermissions.List);
            // sasBuilder.SetPermissions(B)
            sasBuilder.IPRange = SasIPRange.Parse(remoteIp);

            Uri sasUri = containerClient.GenerateSasUri(sasBuilder);
            _logger.LogInformation($"SAS URI for blob container is: {sasUri}");
            

            return sasUri;
        }
        else
        {
            _logger.LogInformation(@"BlobContainerClient must be authorized with Shared Key 
                            credentials to create a service SAS.");
            return null;
        }
    }


    private string FindRemoteIp(){
        
        string remoteIp = DEFAULT_IP;
        IHeaderDictionary? headers = HttpContext.Request?.Headers;
        if( headers!=null){
            string xenvoy = headers[XENVOY_IP];
            string xfwd4 = headers[XFWD4_IP];
            remoteIp = (xenvoy!=null && xenvoy.Equals(xfwd4))?xenvoy:remoteIp;
            _logger.LogInformation($"xenvoy={xenvoy}, xfwd4={xfwd4}");
            foreach (var itm in headers)
            {                
                _logger.LogInformation($"header item {itm.Key} - {itm.Value}|");
            }
        }
        else
        {
            _logger.LogError($"The IPs do not match, cannot create token");
        }
            
        
        return remoteIp;

        
    }

}

