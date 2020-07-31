using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SiteAccelerator
{
    class HostedService : BackgroundService
    {
        private readonly IServiceProvider service;
        private readonly ILogger<HostedService> logger;

        public HostedService(IServiceProvider service, ILogger<HostedService> logger)
        {
            this.service = service;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (stoppingToken.IsCancellationRequested == false)
            {
                try
                {
                    await this.RefleshIPAddressAsync();
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, ex.Message);
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromMinutes(2d));
                }
            }
        }


        private async Task RefleshIPAddressAsync()
        {
            using var scope = this.service.CreateScope();
            var ip138Api = scope.ServiceProvider.GetService<IIp138Api>();
            var siteTestApi = scope.ServiceProvider.GetService<ISiteTestApi>();
            var sites = scope.ServiceProvider.GetService<IOptionsSnapshot<SitesOptions>>().Value;

            foreach (var site in sites)
            {
                this.logger.LogInformation($"正在解析域名{site.Host}");
                var ip138Result = await ip138Api.ReadAsync(site.Host);
                if (ip138Result.Status == false)
                {
                    continue;
                }

                var testResults = new List<TestResult>();
                foreach (var ipItem in ip138Result.Data)
                {
                    var uri = ipItem.ToIpUri(site);
                    var testResult = new TestResult(ipItem.Ip);

                    try
                    {
                        this.logger.LogInformation($"正在请求到{uri}");
                        await siteTestApi.GetAsync(uri, site.Host);
                        testResults.Add(testResult);
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError($"请求{uri}异常：{ex.Message}");
                    }
                    finally
                    {
                        testResult.Finish();
                        this.logger.LogInformation($"请求{uri}耗时：{testResult.Elapsed}");
                    }
                }

                var first = testResults
                    .OrderBy(item => item.Elapsed)
                    .FirstOrDefault();

                if (first != null)
                {
                    this.logger.LogInformation($"设置host文件: {first.Ip} {site.Host}");
                    HostsFile.Set(first.Ip, site.Host);
                }
            }
        }
    }
}
