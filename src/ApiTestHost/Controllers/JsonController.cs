using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ApiTestHost.Controllers
{
    [Route("json")]
    public class JsonController : Controller
    {
        private readonly ILogger<JsonController>? _logger;

        public JsonController(ILogger<JsonController>? logger)
        {
            _logger = logger;
        }

        [HttpGet("{*url}")]
        public async Task<IActionResult> GetJson(string url)
        {
            if (!TryGetFilePath(url, out var filePath))
                return NotFound();

            _logger.LogInformation("Sending requested file: {0}", filePath);
            var json = await System.IO.File.ReadAllTextAsync(filePath);
            return Content(json, "application/json");
        }

        [HttpDelete("{*url}")]
        public IActionResult DeleteJson(string url)
        {
            if (!TryGetFilePath(url, out var filePath))
                return NotFound();

            _logger.LogInformation("Deleting requested file: {0}", filePath);
            System.IO.File.Delete(filePath);
            return Ok();
        }

        [HttpPatch("{*url}")]
        [HttpPost("{*url}")]
        [HttpPut("{*url}")]
        public async Task<IActionResult> WriteJson(string url)
        {
            TryGetFilePath(url, out var filePath, false);

            string json;
            using (var reader = new StreamReader(HttpContext.Request.Body))
                json = await reader.ReadToEndAsync();

            _logger.LogInformation("Writing requested file: {0}", filePath);
            await System.IO.File.WriteAllTextAsync(filePath, json);
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
