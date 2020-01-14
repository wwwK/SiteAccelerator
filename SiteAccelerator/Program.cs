using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebApiClient;

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
                    services.Configure<DomainOptions>(hosting.Configuration.GetSection(nameof(DomainOptions)));
                    services.AddHttpApi<Iip138Api>();
                    services.AddHostedService<HostedService>();
                });
        }
    }
}
