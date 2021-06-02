using ApiTestHost.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
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

            var request = HttpContext.CreateProxyHttpRequest(targetUri);

            _logger.LogInformation("Sending request to \"{0}\"...", url);
            var response = await _httpClient.SendAsync(request);
            _logger.LogInformation("Got response from \"{0}\". Redirecting response...", url);
            await HttpContext.CopyProxyHttpResponse(response);
        }
    }
}