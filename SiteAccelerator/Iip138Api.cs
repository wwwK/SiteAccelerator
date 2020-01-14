using System.Threading.Tasks;
using WebApiClient;
using WebApiClient.Attributes;

namespace SiteAccelerator
{
    [HttpHost("https://site.ip138.com/")]
    [TraceFilter(OutputTarget = OutputTarget.LoggerFactory)]
    public interface Iip138Api : IHttpApi
    {
        [JsonReturn]
        [HttpGet("domain/read.do")]
        Task<ApiResult<IpItem>> ReadAsync(string domain);
    }
}
