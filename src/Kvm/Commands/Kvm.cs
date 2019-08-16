using System;
using System.Threading.Tasks;
using Kvm.Application;
using McMaster.Extensions.CommandLineUtils;

namespace Kvm.Commands
{
    [Command("kvm")]
    [Subcommand(
        typeof(Install),
        typeof(ListInstalled),
        typeof(ListRemote),
        typeof(Refresh),
        typeof(Remove),
        typeof(UseVersion))]
    internal class Kvm
    {
        public async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            app.ShowHelp(false);
            var version = await DataCache.GetKubectlVersionInUse();
            Console.WriteLine($"Actual version in use: {version}");
            return 1;
        }
    }
}
