using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Kurukuru;
using Kvm.Application;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Semver;

namespace Kvm.Commands
{
    [Command("refresh", Description = "Refreshes the local datacache of kubectl versions.")]
    internal class Refresh
    {
        private const string? InitialUrl = "https://api.github.com/repos/kubernetes/kubernetes/releases?per_page=100";
        private readonly HttpClient _client = new HttpClient();

        public Refresh()
        {
            _client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("KubectlVersionManager", "1"));
        }

        public async Task<int> OnExecuteAsync()
        {
            var url = InitialUrl;
            var definition = new[] { new { tag_name = "" } };
            var versions = new List<SemVersion>();

            using var spinner = new Spinner("Refresh kubectl versions.");
            spinner.Start();

            while (url != null)
            {
                spinner.Text = $"Refresh kubectl versions from: {url}";
                var response = await _client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var releases = JsonConvert.DeserializeAnonymousType(
                    await response.Content.ReadAsStringAsync(),
                    definition);

                versions.AddRange(
                    releases.Select(r => r.tag_name.Substring(1)).Select(tag => SemVersion.Parse(tag)));

                url = response.Headers.GetValues("Link")
                    .First()
                    .Split(',')
                    .Select(
                        link => Regex.Match(
                            link,
                            "<(.*?)>; rel=\"next\"",
                            RegexOptions.IgnoreCase | RegexOptions.Multiline))
                    .Where(m => m.Success)
                    .Select(m => m.Groups[1].Value)
                    .FirstOrDefault();
            }

            spinner.Succeed();
            await DataCache.SetKubectlVersions(versions);
            Console.WriteLine("Local version cache updated.");

            return 0;
        }
    }
}
