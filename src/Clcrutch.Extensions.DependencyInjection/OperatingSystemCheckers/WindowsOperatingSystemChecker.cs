using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clcrutch.Extensions.DependencyInjection.OperatingSystemCheckers
{
    internal class WindowsOperatingSystemChecker
    {
        public bool Test() =>
            OperatingSystem.IsWindows();
    }
}
