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
            var version = await DataCache.GetKubectlVersionInUse();
            app.ExtendedHelpText = $"Actual version in use: {version}";
            app.ShowHelp(false);
            return ExitCodes.Error;
        }
    }
}
