using System.Threading.Tasks;
using WebApiClientCore;
using WebApiClientCore.Attributes;

namespace SiteAccelerator
{
    [EmptyUserAgent]   
    [HttpHost("https://site.ip138.com/")]
    [JsonReturn(EnsureMatchAcceptContentType = false)]
    public interface IIp138Api
    {
        [HttpGet("domain/read.do")]       
        ITask<ApiResult<IpItem>> ReadAsync(string domain);
    }

    class EmptyUserAgentAttribute : ApiFilterAttribute
    {
        public override Task OnRequestAsync(ApiRequestContext context)
        {
            context.HttpContext.RequestMessage.Headers.UserAgent.Clear();
            return Task.CompletedTask;
        }

        public override Task OnResponseAsync(ApiResponseContext context)
        {
            return Task.CompletedTask;
        }
    }
}
