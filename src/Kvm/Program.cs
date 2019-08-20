using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Kvm.Application.Platforms;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;

namespace Kvm
{
    internal static class Program
    {
        public static Task<int> Main(string[] args)
        {
            var app = new CommandLineApplication<Commands.Kvm>();

            var services = new ServiceCollection()
                .AddSingleton(PhysicalConsole.Singleton);

            switch (true)
            {
                case true when RuntimeInformation.IsOSPlatform(OSPlatform.Linux):
                    services.AddTransient<IPlatform, Linux>();
                    break;
                case true when RuntimeInformation.IsOSPlatform(OSPlatform.OSX):
                    services.AddTransient<IPlatform, MacOs>();
                    break;
                default:
                    throw new NotSupportedException("OS not supported.");
            }

            app
                .Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(services.BuildServiceProvider());

            return app.ExecuteAsync(args);
        }
    }
}
