using System;

namespace SiteAccelerator
{
    public class IpItem
    {
        public string Ip { get; set; }

        public string Sign { get; set; }

        public Uri ToIpUri(Uri site)
        {
            var uri = $"{site.Scheme}://{Ip}:{site.Port}{site.PathAndQuery}";
            return new Uri(uri);
        } 
    }
}
