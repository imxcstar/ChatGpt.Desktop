using DALLE2.API;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var proxy = Environment.GetEnvironmentVariable("http_proxy");
var proxys = Environment.GetEnvironmentVariable("https_proxy");
if (!string.IsNullOrWhiteSpace(proxys))
    HttpClient.DefaultProxy = new WebProxy(proxys);
else if (!string.IsNullOrWhiteSpace(proxy))
    HttpClient.DefaultProxy = new WebProxy(proxy);

DefConfig.ApiKey = Environment.GetEnvironmentVariable("API_KEY");

if (!Directory.Exists(DefConfig.ImagePath))
    Directory.CreateDirectory(DefConfig.ImagePath);

var image_path = Environment.GetEnvironmentVariable("IMAGE_PATH");
if (!string.IsNullOrWhiteSpace(image_path))
{
    if (!Directory.Exists(image_path))
        throw new Exception($"IMAGE_PATH '{image_path}' does not exist");
    DefConfig.ImagePath = image_path;
}

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();
