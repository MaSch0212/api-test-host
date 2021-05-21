using ApiTestHost.Middlewares;
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

        public DelayController(IHttpClientFactory clientFactory, ILogger<DelayController>? logger)
        {
            _httpClient = clientFactory.CreateClient("HttpClientWithSSLUntrusted");
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
            HttpContext.Items.Add(CopyResponseMiddleware.CopyKey, response);
        }
    }
}