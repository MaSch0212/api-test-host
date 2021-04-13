using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiTestHost.Controllers
{
    [Route("json")]
    [SwaggerTag("Provides functionality of a JSON Server")]
    public class JsonController : Controller
    {
        private readonly ILogger<JsonController>? _logger;

        public JsonController(ILogger<JsonController>? logger)
        {
            _logger = logger;
        }

        [SwaggerOperation("Gets the specified json document.")]
        [SwaggerResponse(StatusCodes.Status200OK, "The document was found and the content is returned.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The document was not found.")]
        [HttpGet("{*url}")]
        public async Task<IActionResult> GetJson(
            [SwaggerParameter("The path to the json document to get.")]
            string url)
        {
            if (!TryGetFilePath(url, out var filePath))
                return NotFound();

            _logger.LogInformation("Sending requested file: {0}", filePath);
            var json = await System.IO.File.ReadAllTextAsync(filePath);
            return Content(json, "application/json");
        }

        [SwaggerOperation("Deletes the specified json document.")]
        [SwaggerResponse(StatusCodes.Status200OK, "The document was found and deleted.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The document was not found.")]
        [HttpDelete("{*url}")]
        public IActionResult DeleteJson(string url)
        {
            if (!TryGetFilePath(url, out var filePath))
                return NotFound();

            _logger.LogInformation("Deleting requested file: {0}", filePath);
            System.IO.File.Delete(filePath);
            return Ok();
        }

        [Route("{*url}")]
        [SwaggerOperation("Adds or overwrites the specified json document.")]
        [SwaggerResponse(StatusCodes.Status200OK, "The document has been created or overwritten.")]
        [HttpPatch]
        [HttpPost]
        [HttpPut]
        public async Task<IActionResult> WriteJson(
            string url,
            [SwaggerRequestBody("The content of the json document", Required = true)]
            [FromBody] object json)
        {
            TryGetFilePath(url, out var filePath, false);

            var strJson = JsonSerializer.Serialize(json, new JsonSerializerOptions { WriteIndented = true });

            _logger.LogInformation("Writing requested file: {0}", filePath);
            await System.IO.File.WriteAllTextAsync(filePath, strJson);
            return Ok();
        }

        private bool TryGetFilePath(string url, out string filePath, bool logNotFound = true)
        {
            filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "json", url + ".json");
            if (!System.IO.File.Exists(filePath))
            {
                if (logNotFound)
                    _logger.LogError("Could not find requested file: {0}", filePath);
                return false;
            }

            return true;
        }
    }
}
