using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Kvm.Application.Platforms
{
    public class Windows : IPlatform
    {
        public string Name => "windows";
        public string FileExtension => ".exe";

        [DllImport("kernel32.dll")]
        private static extern bool CreateSymbolicLink(
            string lpSymlinkFileName,
            string lpTargetFileName,
            int dwFlags);

        private string LinkFolder => Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "kvm");

        public string KubectlLinkPosition => Path.Join(
            LinkFolder,
            "kubectl.exe");

        public Task MakeFileExecutable(string filePath)
        {
            var security = new FileSecurity(
                filePath,
                AccessControlSections.Owner | AccessControlSections.Group | AccessControlSections.Access);
            var owner = security.GetOwner(typeof(NTAccount));

            security.SetAccessRule(
                new FileSystemAccessRule(owner, FileSystemRights.ExecuteFile, AccessControlType.Allow));
            return Task.CompletedTask;
        }

        public Task LinkKubectlVersion(string kubectlFile)
        {
            if (!Directory.Exists(LinkFolder))
            {
                Directory.CreateDirectory(LinkFolder);
            }

            if (File.Exists(KubectlLinkPosition))
            {
                File.Delete(KubectlLinkPosition);
            }

            var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            if (!path.Contains(LinkFolder))
            {
                Console.WriteLine($"WARNING: please add the '{LinkFolder}' directory to your PATH variable.");
            }

            return !CreateSymbolicLink(KubectlLinkPosition, kubectlFile, 0x0 | 0x2)
                ? Task.FromException(new ApplicationException("Could not create symbolic link."))
                : Task.CompletedTask;
        }
    }
}
