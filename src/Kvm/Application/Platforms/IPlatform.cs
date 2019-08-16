using System.Threading.Tasks;

namespace Kvm.Application.Platforms
{
    internal interface IPlatform
    {
        string Name { get; }

        string FileExtension { get; }

        string KubectlLinkPosition { get; }

        Task MakeFileExecutable(string filePath);

        Task LinkKubectlVersion(string kubectlFile);
    }
}
