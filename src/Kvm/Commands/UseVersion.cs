using System;
using System.IO;
using System.Threading.Tasks;
using Kvm.Application;
using McMaster.Extensions.CommandLineUtils;
using Semver;

namespace Kvm.Commands
{
    [Command("use", "u", Description = "Locally links the given semver version to be used by the system.")]
    internal class UseVersion
    {
        private const string Quit = "q";

        [Argument(0, "semver", "Optionally specify the semver version to link.")]
        public string? VersionToUse { get; set; }

        public async Task<int> OnExecuteAsync()
        {
            VersionToUse ??= Prompt.GetString("Enter semver version that you want to use ('q' to exit):");
            if (VersionToUse == Quit)
            {
                return 130;
            }

            while (!SemVersion.TryParse(VersionToUse, out _))
            {
                Console.WriteLine($"'{VersionToUse}' is not a valid semver. Please try again.");
                VersionToUse = Prompt.GetString("Enter semver version that you want to use ('q' to exit):");

                if (VersionToUse == Quit)
                {
                    return 130;
                }
            }

            var version = DataCache.GetInstalledKubectlVersions().MaxSatisfying(VersionToUse ?? "");

            if (version == null)
            {
                Console.WriteLine($"No matching version found with version string '{VersionToUse}'.");
                return 1;
            }

            var versionDirectory = Path.Join(DataCache.VersionsDir, version.ToString());
            var versionFile = Path.Join(versionDirectory, $"kubectl{PlatformSpecifics.Platform.FileExtension}");

            if(!(Directory.Exists(versionDirectory) && File.Exists(versionFile)))
            {
                Console.WriteLine("Version is not installed. Please use kvm install ... to install it.");
                return 1;
            }

            Console.WriteLine($"Linking local binary (v{version}) for usage.");
            await PlatformSpecifics.Platform.LinkKubectlVersion(versionFile);
            await DataCache.SetKubectlVersionInUse(version);

            return 0;
        }
    }
}
