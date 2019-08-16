using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Kurukuru;
using Kvm.Application;
using McMaster.Extensions.CommandLineUtils;
using Semver;

namespace Kvm.Commands
{
    [Command("install", "i", Description = "Install (and use) a specific version of kubectl locally.")]
    internal class Install
    {
        private const string Quit = "q";
        private readonly HttpClient _client = new HttpClient();

        [Argument(0, "semver", "Optionally specify the semver version to install.")]
        public string? InstallVersion { get; set; }

        [Option("--no-use", Description = "If specified, the installed version is not directly used afterwards.")]
        public bool DontUseDirectly { get; set; }

        public async Task<int> OnExecuteAsync()
        {
            InstallVersion ??= Prompt.GetString("Enter semver version that you want to install ('q' to exit):");
            if (InstallVersion == Quit)
            {
                return 130;
            }

            while (!SemVersion.TryParse(InstallVersion, out _))
            {
                Console.WriteLine($"'{InstallVersion}' is not a valid semver. Please try again.");
                InstallVersion = Prompt.GetString("Enter semver version that you want to install ('q' to exit):");
                if (InstallVersion == Quit)
                {
                    return 130;
                }
            }

            var version = (await DataCache.GetKubectlVersions().ToListAsync()).MaxSatisfying(InstallVersion ?? "");

            if (version == null)
            {
                Console.WriteLine($"No matching version found with version string '{InstallVersion}'.");
                return 1;
            }

            var versionDirectory = Path.Join(DataCache.VersionsDir, version.ToString());
            var versionFile = Path.Join(versionDirectory, $"kubectl{PlatformSpecifics.Platform.FileExtension}");
            if (Directory.Exists(versionDirectory) && File.Exists(versionFile))
            {
                Console.WriteLine($"kubectl v{version} is already installed.");
                return 1;
            }

            Console.WriteLine($"Download kubectl from: {KubectlDownloadUrl(version)}.");
            using var spinner = new Spinner($"Install kubectl v{version}");
            spinner.Start();

            try
            {
                Directory.CreateDirectory(versionDirectory);
                var response = await _client.GetStreamAsync(KubectlDownloadUrl(version));
                await using var file = File.OpenWrite(versionFile);
                await response.CopyToAsync(file);
                await PlatformSpecifics.Platform.MakeFileExecutable(file.Name);

                spinner.Succeed();
            }
            catch (Exception e)
            {
                spinner.Fail();
                Directory.Delete(versionDirectory, true);
                Console.WriteLine($"Error: {e}");
                return 1;
            }

            if (DontUseDirectly)
            {
                return 0;
            }

            var useVersion = new UseVersion { VersionToUse = InstallVersion };
            return await useVersion.OnExecuteAsync();
        }

        private static string KubectlDownloadUrl(SemVersion version) =>
            $"https://storage.googleapis.com/kubernetes-release/release/v{version}/bin/{PlatformSpecifics.Platform.Name}/amd64/kubectl{PlatformSpecifics.Platform.FileExtension}";
    }
}
