using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;

namespace SiteAccelerator
{
    class Program
    {
        /// <summary>
        /// 程序入口
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            if (WindowsService.UseCtrlCommand() == false)
            {
                CreateHostBuilder(args).Build().Run();
            }
        }

        /// <summary>
        /// 创建host
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host
                .CreateDefaultBuilder(args)
                .UseBinPathEnvironment()
                .UseWindowsService()
                .ConfigureServices((hosting, services) =>
                {
                    services.Configure<SitesOptions>(hosting.Configuration.GetSection(nameof(SitesOptions)));
                    services.AddHttpApi<IIp138Api>();
                    services.AddHttpApi<ISiteTestApi>().ConfigurePrimaryHttpMessageHandler(() =>
                    {
                        return new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = (a, b, c, d) => true
                        };
                    });
                    services.AddHostedService<HostedService>();
                });
        }
    }
}
