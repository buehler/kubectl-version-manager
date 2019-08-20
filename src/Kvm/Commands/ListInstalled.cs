using System;
using System.Threading.Tasks;
using Kvm.Application;
using McMaster.Extensions.CommandLineUtils;

namespace Kvm.Commands
{
    [Command("list", "ls", Description = "Lists local installed versions.")]
    internal class ListInstalled
    {
        public Task<int> OnExecuteAsync()
        {
            Console.WriteLine("Found the following versions installed locally:");
            foreach (var version in DataCache.GetInstalledKubectlVersions())
            {
                Console.WriteLine(version.ToString());
            }

            return Task.FromResult(ExitCodes.Success);
        }
    }
}
