using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage;
using System.Net;
using Microsoft.AspNetCore.Http.Features;

namespace file_server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SaSTokenController : ControllerBase
{
    private readonly ILogger<SaSTokenController> _logger;
    IConfiguration _configuration;
    
    private readonly string XENVOY_IP = "X-Envoy-External-Address";
    private readonly string XFWD4_IP = "x-forwarded-for";
    public SaSTokenController(ILogger<SaSTokenController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
 
    }

    [HttpGet(Name = "GetSaSToken")]
    public string GetSaSToken()
    {                     
        SharedAccessBlobPolicy accessPolicy = new SharedAccessBlobPolicy
        {
            // Define expiration to be 30 minutes from now in UTC
            SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["access_period"])),
            // Add permissions
            Permissions = SharedAccessBlobPermissions.Create | SharedAccessBlobPermissions.Write
        };
        var feature = HttpContext.Features.Get<IHttpConnectionFeature>();
        var remoteIp = IPAddress.Parse(FindRemoteIp());
        
        string connectionString = _configuration.GetValue<string>("storagecs");
        string containerName = Guid.NewGuid().ToString();

        // get the ip address of the caller and set it

        string secretValue = _configuration.GetValue<string>("test-secret");
        string token = Guid.NewGuid().ToString(); 
        return $"Container: {containerName} |Period - {_configuration["access_period"]} - ip {remoteIp.MapToIPv6()} |";
    }

    private string FindRemoteIp(){
        
        string remoteIp = "1.1.1.1";
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
        
        return remoteIp;

        
    }

}

