using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace ApiTestHost.Middlewares
{
    public class CopyResponseMiddleware
    {
        public const string CopyKey = "ResponseItem";
        private readonly RequestDelegate _next;

        public CopyResponseMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            await _next(httpContext);
            await CopyResponseAsync(httpContext);
        }

        private async Task CopyResponseAsync(HttpContext httpContext)
        {
            if (!httpContext.Items.ContainsKey(CopyKey))
                return;
            var response = httpContext.Items[CopyKey] as HttpResponseMessage;
            if (response == null)
                return;

            var resp = httpContext.Response;

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