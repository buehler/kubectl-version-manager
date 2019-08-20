using System;
using System.IO;
using System.Threading.Tasks;
using Kvm.Application;
using Kvm.Application.Platforms;
using McMaster.Extensions.CommandLineUtils;
using Semver;

namespace Kvm.Commands
{
    [Command("remove", "rm", "del", Description = "Deletes a locally used version from the system.")]
    internal class Remove
    {
        private readonly IPlatform _platform;
        private const string Quit = "q";

        [Argument(0, "semver", "Optionally specify the semver version to remove.")]
        public string? VersionToUse { get; set; }

        public Remove(IPlatform platform)
        {
            _platform = platform;
        }

        public async Task<int> OnExecuteAsync()
        {
            VersionToUse ??= Prompt.GetString("Enter semver version that you want to remove ('q' to exit):");
            if (VersionToUse == Quit)
            {
                return ExitCodes.CannotProceed;
            }

            while (!SemVersion.TryParse(VersionToUse, out _))
            {
                Console.WriteLine($"'{VersionToUse}' is not a valid semver. Please try again.");
                VersionToUse = Prompt.GetString("Enter semver version that you want to remove ('q' to exit):");

                if (VersionToUse == Quit)
                {
                    return ExitCodes.CannotProceed;
                }
            }

            var version = DataCache.GetInstalledKubectlVersions().MaxSatisfying(VersionToUse ?? "");

            if (version == null)
            {
                Console.WriteLine($"No matching version found with version string '{VersionToUse}'.");
                return ExitCodes.Error;
            }

            var versionDirectory = Path.Join(DataCache.VersionsDir, version.ToString());
            var versionFile = Path.Join(versionDirectory, $"kubectl{_platform.FileExtension}");

            if (!(Directory.Exists(versionDirectory) && File.Exists(versionFile)))
            {
                Console.WriteLine("Version is not installed.");
                return ExitCodes.Error;
            }

            Console.WriteLine($"Remove local binary (v{version}).");
            Directory.Delete(versionDirectory, true);

            if (version != await DataCache.GetKubectlVersionInUse())
            {
                return ExitCodes.Success;
            }

            Console.WriteLine("The removed version was in use, the symlink is deleted as well.");
            File.Delete(_platform.KubectlLinkPosition);

            return ExitCodes.Success;
        }
    }
}
