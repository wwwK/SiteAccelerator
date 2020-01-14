using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace SiteAccelerator
{
    public class IpItem
    {
        public string Ip { get; set; }

        public string Sign { get; set; }

        public async Task<PingReply> PingAsync()
        {
            using var ping = new Ping();
            return await ping.SendPingAsync(this.Ip);
        }
    }
}
