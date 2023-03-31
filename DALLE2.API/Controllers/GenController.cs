using Microsoft.AspNetCore.Mvc;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels;
using SixLabors.ImageSharp.Formats.Png;
using System;
using SixLabors.ImageSharp;
using OpenAI.GPT3;

namespace DALLE2.API.Controllers
{
    [ApiController]
    [Route("/")]
    public class GenController : ControllerBase
    {
        public GenController()
        {
        }

        [HttpGet]
        [Route("{size}")]
        [ProducesResponseType(200, Type = typeof(FileStreamResult))]
        public async Task<IActionResult> Get([FromRoute] string size, [FromQuery] string prompt, [FromQuery] string? id = null, [FromQuery] string? apikey = null)
        {
            try
            {
                if (size != "256x256" && size != "512x512" && size != "1024x1024")
                    return new BadRequestResult();

                var isRead = false;
                if (!string.IsNullOrWhiteSpace(id))
                {
                    if (!Guid.TryParse(id, out _))
                        return new BadRequestResult();
                    isRead = true;
                }
                else
                {
                    id = Guid.NewGuid().ToString();
                }

                var file = Path.Combine(DefConfig.ImagePath, $"{id}.png");
                if (isRead)
                {
                    if (System.IO.File.Exists(file))
                    {
                        var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                        return new FileStreamResult(fs, "image/png");
                    }
                }

                var tapikey = "";
                if (!string.IsNullOrWhiteSpace(apikey))
                    tapikey = apikey;
                else if (!string.IsNullOrWhiteSpace(DefConfig.ApiKey))
                    tapikey = DefConfig.ApiKey;
                else if (Request.Cookies.ContainsKey("APIKEY"))
                    tapikey = Request.Cookies["APIKEY"];

                if (string.IsNullOrWhiteSpace(tapikey))
                    return new UnauthorizedResult();

                var openAIService = new OpenAIService(new OpenAiOptions()
                {
                    ApiKey = tapikey
                });

                var imageResult = await openAIService.Image.CreateImage(new ImageCreateRequest
                {
                    Prompt = prompt,
                    N = 1,
                    Size = size,
                    ResponseFormat = StaticValues.ImageStatics.ResponseFormat.Base64,
                });

                if (!imageResult.Successful)
                    return new BadRequestResult();

                var imageBytes = Convert.FromBase64String(imageResult.Results.First().B64);

                var ret = new MemoryStream();
                using var image = Image.Load(imageBytes);
                image.Save(ret, PngFormat.Instance);
                image.SaveAsPng(file);
                ret.Position = 0;
                return new FileStreamResult(ret, "image/png");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new BadRequestResult();
            }
        }
    }
}