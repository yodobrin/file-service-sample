using Microsoft.AspNetCore.Mvc;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Core;

namespace file_server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SaSTokenController : ControllerBase
{
    private readonly ILogger<SaSTokenController> _logger;
    private readonly SecretClientOptions options = new SecretClientOptions()
            {
                Retry =
                {
                    Delay= TimeSpan.FromSeconds(2),
                    MaxDelay = TimeSpan.FromSeconds(16),
                    MaxRetries = 5,
                    Mode = RetryMode.Exponential
                }
            };
    private SecretClient secretClient;
    public SaSTokenController(ILogger<SaSTokenController> logger)
    {
        _logger = logger;
        // change this to be from config
        var miCredentials = new DefaultAzureCredential(new DefaultAzureCredentialOptions
                        {
                            ManagedIdentityClientId = "7e07a243-0b11-490b-9ff5-bd188b369cc7"
                        });
        secretClient = new SecretClient(new Uri("https://fileuploadkv.vault.azure.net/"), 
                                        new DefaultAzureCredential(),
                                        options);
    }

    [HttpGet(Name = "GetSaSToken")]
    public string GetSaSToken()
    {             
        KeyVaultSecret secret = secretClient.GetSecret("test-secret");
        string secretValue = secret.Value;
        
        string token = Guid.NewGuid().ToString(); 
        return $"this is a very super random token: {token} - {secretValue}";
    }



}

// fedc565b565d2cffdf3a1a1a2290de4c1aef5b95
// fedc565b565d2cffdf3a1a1a2290de4c1aef5b95