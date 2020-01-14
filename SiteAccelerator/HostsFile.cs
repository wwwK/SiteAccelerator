using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace SiteAccelerator
{
    public static class HostsFile
    {
        private static readonly string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts");

        private static List<HostItem> ReadHostItems()
        {
            return File.ReadAllLines(path, Encoding.UTF8).Select(item => new HostItem(item)).ToList();
        }

        private static void WriteHostItems(IEnumerable<HostItem> items)
        {
            var builder = new StringBuilder();
            foreach (var item in items)
            {
                builder.AppendLine(item.ToString());
            }

            File.SetAttributes(path, FileAttributes.Normal);
            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        }

        public static void Set(IPAddress ip, string domain)
        {
            var items = ReadHostItems();
            var item = items.Find(item => string.Equals(item.Domain, domain, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                items.Remove(item);
            }

            item = new HostItem(ip, domain);
            items.Add(item);
            WriteHostItems(items);
        }

        private class HostItem
        {
            private readonly string rawString;

            public bool IsNote => rawString != null && rawString.TrimStart(' ').StartsWith('#');

            public bool IsNullOrEmpty => string.IsNullOrWhiteSpace(rawString);

            public string Domain
            {
                get
                {
                    if (this.IsNullOrEmpty || this.IsNote)
                    {
                        return null;
                    }

                    var span = this.rawString.AsSpan().TrimEnd(' ');
                    var index = span.LastIndexOf(' ');
                    return index < 0 ? null : new string(span.Slice(index + 1));
                }
            }

            public HostItem(string raw)
            {
                this.rawString = raw;
            }
            public HostItem(IPAddress address, string domain)
                : this($"{address} {domain}")
            {
            }

            public override string ToString()
            {
                return this.rawString;
            }
        }
    }
}
