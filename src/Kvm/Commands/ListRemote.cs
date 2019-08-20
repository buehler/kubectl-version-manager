using System.Threading.Tasks;
using Kvm.Application;
using McMaster.Extensions.CommandLineUtils;

namespace Kvm.Commands
{
    [Command(
        "list-remote",
        "ls-remote",
        "lsr",
        Description = "Lists all versions that are available online (but from local data cache).")]
    internal class ListRemote
    {
        public async Task<int> OnExecuteAsync()
        {
            using var pager = new Pager
                { Prompt = "Remote available versions\\. (From local cache-file)\\. Press 'q' to exit\\." };
            await foreach (var version in DataCache.GetKubectlVersions())
            {
                await pager.Writer.WriteLineAsync(version.ToString());
            }

            return ExitCodes.Success;
        }
    }
}
