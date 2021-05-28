using DependencyManager.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyManager.Console.Commands
{
    class TestCommand
    {
        private async Task<int> OnExecuteAsync()
        {
            var executor = new Executor();
            if (await executor.TestInstallNeededAsync())
            {
                System.Console.WriteLine("There are dependency packages missing.  Please run depend install to install the missing packages.");
                return 1;
            }

            return 0;
        }
    }
}
