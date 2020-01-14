using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
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
            var api = scope.ServiceProvider.GetService<Iip138Api>();
            var domains = scope.ServiceProvider.GetService<IOptionsSnapshot<DomainOptions>>().Value;

            foreach (var domain in domains)
            {
                var apiResult = await api.ReadAsync(domain);
                if (apiResult.Status == false)
                {
                    continue;
                }

                var pingResults = new List<PingReply>();
                foreach (var item in apiResult.Data)
                {
                    try
                    {
                        var pingReply = await item.PingAsync();
                        this.logger.LogInformation($"ping {item.Ip} -> {pingReply.Status} {pingReply.RoundtripTime}");
                        pingResults.Add(pingReply);
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogWarning($"ping error at {item.Ip}: {ex.Message}");
                    }
                }

                var first = pingResults.Where(item => item.Status == IPStatus.Success).OrderBy(item => item.RoundtripTime).FirstOrDefault();
                if (first != null)
                {
                    this.logger.LogInformation($"set hosts: {first.Address} {domain}");
                    HostsFile.Set(first.Address, domain);
                }
            }
        }
    }
}
