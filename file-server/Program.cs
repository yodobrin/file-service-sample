using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// when running localy and your user is granted right rbac on the keyvault, u can use this:
// builder.Configuration.AddAzureKeyVault(
//         new Uri($"https://{builder.Configuration["keyvault"]}.vault.azure.net/"),
//         new DefaultAzureCredential());

builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{builder.Configuration["keyvault"]}.vault.azure.net/"),
        new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            ManagedIdentityClientId = builder.Configuration["AzureADManagedIdentityClientId"]
        }));


// Add services to the container
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();


// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
