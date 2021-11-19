using System.Runtime.InteropServices;

namespace Clcrutch.Extensions.DependencyInjection.OperatingSystemCheckers
{
    internal class LinuxOperatingSystemChecker
    {
        public bool Test() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }
}
