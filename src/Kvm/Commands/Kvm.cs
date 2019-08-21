using System;
using System.Reflection;
using System.Threading.Tasks;
using Kvm.Application;
using McMaster.Extensions.CommandLineUtils;

namespace Kvm.Commands
{
    [Command("kvm")]
    [VersionOptionFromMember("-v|--version", MemberName = nameof(VersionOption))]
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
            app.ExtendedHelpText = $"Actual kubectl version in use: {version}";
            app.ShowHelp(false);
            return ExitCodes.Error;
        }

        private static string VersionOption => $"kvm ver. {Version}";

        private static string Version => typeof(Kvm)
                                             .Assembly
                                             .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                             ?
                                             .InformationalVersion ??
                                         "No Version Defined.";
    }
}
