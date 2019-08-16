using System;
using System.Runtime.InteropServices;
using Kvm.Application.Platforms;

namespace Kvm.Application
{
    internal static class PlatformSpecifics
    {
        private static readonly IPlatform Linux = new Linux();

        public static IPlatform Platform => true switch
        {
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => Linux,
            _ => throw new NotSupportedException("OS not supported."),
        };
    }
}
