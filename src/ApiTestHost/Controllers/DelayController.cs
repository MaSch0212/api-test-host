using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace ApiTestHost.Controllers
{
    [Route("delay")]
    [SwaggerTag("Provides functionality of a Delay Server")]
    public class DelayController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DelayController>? _logger;

        public DelayController(ILogger<DelayController>? logger)
        {
            _httpClient = new HttpClient();
            _logger = logger;
        }

        [SwaggerOperation("Calls the specified url with the request information of this http request after the specified delay time.")]
        [Route("{delay}/{*url}")]
        [HttpDelete]
        [HttpGet]
        [HttpHead]
        [HttpOptions]
        [HttpPatch]
        [HttpPost]
        [HttpPut]
        public async Task Index(
            [SwaggerParameter("The delay time in milliseconds.")]
            int delay,
            [SwaggerParameter("The url to call.")]
            string url)
        {
            _logger.LogInformation("Delaying request to \"{0}\" by {1} milliseconds...", url, delay);
            await Task.Delay(delay);

            var targetUri = new UriBuilder(HttpUtility.UrlDecode(url)) { Query = HttpContext.Request.QueryString.Value }.Uri;

            var req = HttpContext.Request;
            var request = new HttpRequestMessage(new HttpMethod(req.Method), targetUri);
            foreach (var header in req.Headers)
                request.Headers.TryAddWithoutValidation(header.Key, header.Value.AsEnumerable());
            request.Content = new StreamContent(req.Body);
            if (req.Headers.ContainsKey("Content-Type"))
                request.Content.Headers.Add("Content-Type", req.ContentType);

            _logger.LogInformation("Sending request to \"{0}\"...", url);
            var response = await _httpClient.SendAsync(request);
            _logger.LogInformation("Got response from \"{0}\". Redirecting response...", url);

            var resp = HttpContext.Response;
            resp.StatusCode = (int)response.StatusCode;
            foreach (var header in response.Headers)
                resp.Headers[header.Key] = new StringValues(header.Value.ToArray());
            if (response.Content.Headers.Contains("Content-Type"))
                resp.ContentType = response.Content.Headers.GetValues("Content-Type").Single();
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var bodyStream = resp.BodyWriter.AsStream())
            {
                await stream.CopyToAsync(bodyStream);
            }
        }
    }
}
