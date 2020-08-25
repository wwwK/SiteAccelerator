using WebApiClientCore;
using WebApiClientCore.Attributes;

namespace SiteAccelerator
{
    [HttpHost("https://site.ip138.com/")]
    [JsonReturn(EnsureMatchAcceptContentType = false)]
    public interface IIp138Api
    {
        [HttpGet("domain/read.do")]
        ITask<ApiResult<IpItem>> ReadAsync(string domain);
    }
}
