using System;
using System.Threading.Tasks;
using WebApiClientCore;
using WebApiClientCore.Attributes;

namespace SiteAccelerator
{
    [Timeout(15 * 1000)]
    public interface ISiteTestApi
    {
        [HttpGet]
        [Header(HttpRequestHeader.Accept, "*/*")]
        Task GetAsync([Uri] Uri uri, [Header(HttpRequestHeader.Host)] string host);
    }
}
