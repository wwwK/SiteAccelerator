using System;
using System.Diagnostics;
using System.Net;

namespace SiteAccelerator
{
    class TestResult
    {
        private readonly Stopwatch stopwatch = new Stopwatch();

        public IPAddress Ip { get; }

        public TimeSpan Elapsed => this.stopwatch.Elapsed;

        public TestResult(string ip)
        {
            this.Ip = IPAddress.Parse(ip);
            this.stopwatch.Start();
        }

        public void Finish( )
        {
            this.stopwatch.Stop();
        }
    }
}
