using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Clcrutch.Extensions.DependencyInjection.OperatingSystemCheckers
{
    internal class WindowsOperatingSystemChecker
    {
        public bool Test() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
}
