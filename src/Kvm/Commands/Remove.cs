using System;
using System.IO;
using System.Threading.Tasks;
using Kvm.Application;
using McMaster.Extensions.CommandLineUtils;
using Semver;

namespace Kvm.Commands
{
    [Command("remove", "rm", "del", Description = "Deletes a locally used version from the system.")]
    internal class Remove
    {
        private const string Quit = "q";

        [Argument(0, "semver", "Optionally specify the semver version to remove.")]
        public string? VersionToUse { get; set; }

        public async Task<int> OnExecuteAsync()
        {
            VersionToUse ??= Prompt.GetString("Enter semver version that you want to remove ('q' to exit):");
            if (VersionToUse == Quit)
            {
                return 130;
            }

            while (!SemVersion.TryParse(VersionToUse, out _))
            {
                Console.WriteLine($"'{VersionToUse}' is not a valid semver. Please try again.");
                VersionToUse = Prompt.GetString("Enter semver version that you want to remove ('q' to exit):");

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
                Console.WriteLine("Version is not installed.");
                return 1;
            }

            Console.WriteLine($"Remove local binary (v{version}).");
            Directory.Delete(versionDirectory, true);

            if (version == await DataCache.GetKubectlVersionInUse())
            {
                Console.WriteLine("The removed version was in use, the symlink is deleted as well.");
                File.Delete(PlatformSpecifics.Platform.KubectlLinkPosition);
            }

            return 0;
        }
    }
}
