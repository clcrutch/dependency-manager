using DependencyManager.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyManager
{
    class InstallCommand
    {
        private Task OnExecuteAsync() =>
            new Executor().InstallAsync();
    }
}
