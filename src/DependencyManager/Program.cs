using DependencyManager.Core.Providers;
using DependencyManager.Lib;
using DependencyManager.Providers.Default;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;

namespace DependencyManager
{
    [VersionOption("--version")]
    [Subcommand(typeof(InstallCommand))]
    class Program
    {
        static void Main(string[] args) =>
            CommandLineApplication.Execute<Program>(args);

        private void OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
        }
    }
}
