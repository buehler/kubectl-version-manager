using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Semver;

namespace Kvm.Application
{
    internal static class DataCache
    {
        public static readonly string ConfigDir = Path.Join(
            Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData,
                Environment.SpecialFolderOption.Create),
            "kvm");

        public static readonly string VersionsDir = Path.Join(ConfigDir, "kubectl");

        public static readonly string VersionsFile = Path.Join(VersionsDir, "versions");

        public static readonly string ActualVersionFile = Path.Join(VersionsDir, "version-in-use");

        static DataCache()
        {
            Directory.CreateDirectory(ConfigDir);
            Directory.CreateDirectory(VersionsDir);
        }

        public static async IAsyncEnumerable<SemVersion> GetKubectlVersions()
        {
            if (!File.Exists(VersionsFile))
            {
                yield break;
            }

            using var file = File.OpenText(VersionsFile);
            var data = await file.ReadToEndAsync();

            if (data.Trim() == string.Empty)
            {
                data = "[]";
            }

            foreach (var version in JsonConvert.DeserializeObject<IEnumerable<string>>(data)
                .Select(v => SemVersion.Parse(v))
                .OrderByDescending(v => v))
            {
                yield return version;
            }
        }

        public static async Task SetKubectlVersions(IEnumerable<SemVersion> versions)
        {
            if (File.Exists(VersionsFile))
            {
                File.Delete(VersionsFile);
            }

            await using var file = File.AppendText(VersionsFile);
            await file.WriteAsync(JsonConvert.SerializeObject(versions.Select(v => v.ToString())));
        }

        public static IEnumerable<SemVersion> GetInstalledKubectlVersions() => Directory
            .GetDirectories(VersionsDir)
            .Select(Path.GetFileName)
            .Select(dir => SemVersion.Parse(dir))
            .OrderByDescending(v => v);

        public static async Task<SemVersion> GetKubectlVersionInUse()
        {
            if (!File.Exists(ActualVersionFile))
            {
                return new SemVersion(0);
            }

            using var file = File.OpenText(ActualVersionFile);
            var data = await file.ReadToEndAsync();

            return SemVersion.Parse(data);
        }

        public static async Task SetKubectlVersionInUse(SemVersion version)
        {
            if (File.Exists(ActualVersionFile))
            {
                File.Delete(ActualVersionFile);
            }

            await using var file = File.AppendText(ActualVersionFile);
            await file.WriteAsync(version.ToString());
        }
    }
}
