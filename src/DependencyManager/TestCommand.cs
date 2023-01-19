using DependencyManager.Core;
using DependencyManager.Lib;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyManager
{
    internal class TestCommand
    {
        private async Task<int> OnExecuteAsync()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                //.MinimumLevel.Debug()
                .CreateLogger();

            try
            {
                return await new Executor().TestAsync() ? 0 : 1;
            }
            catch (SuperUserRequiredException)
            {
                Log.Fatal("One or more packages require super user access.  Please restart as super user and try again.");
#if DEBUG
                throw;
#endif
            }
            catch (UserRequiredException)
            {
                Log.Fatal("One ore more packages require normal user access.  Please restart as a normal user and try again.");

#if DEBUG
                throw;
#endif
            }
            catch (InstallFailedException)
            {
                Log.Fatal("One or more installations failed.");

#if DEBUG
                throw;
#endif
            }

            return 1;
        }
    }
}
