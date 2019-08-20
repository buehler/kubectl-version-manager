using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace Kvm.Application.Platforms
{
    internal class MacOs : IPlatform
    {
        public string Name => "darwin";

        public string FileExtension => "";

        public string KubectlLinkPosition => "/usr/local/bin/kubectl";

        public Task MakeFileExecutable(string filePath) =>
            Task.Run(
                () =>
                {
                    using var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "chmod",
                            Arguments = ArgumentEscaper.EscapeAndConcatenate(new[] { "+x", filePath }),
                        },
                    };

                    process.Start();
                    process.WaitForExit(500);
                });

        public async Task LinkKubectlVersion(string kubectlFile)
        {
            if (File.Exists(KubectlLinkPosition))
            {
                File.Delete(KubectlLinkPosition);
            }

            await CreateSymlink(kubectlFile, KubectlLinkPosition);
        }

        private static Task CreateSymlink(string targetFile, string link) =>
            Task.Run(
                () =>
                {
                    using var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "ln",
                            Arguments = ArgumentEscaper.EscapeAndConcatenate(new[] { "-s", targetFile, link }),
                        },
                    };

                    process.Start();
                    process.WaitForExit(500);
                });
    }
}
