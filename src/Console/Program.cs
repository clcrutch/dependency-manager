using DependencyManager.Console.Commands;
using DependencyManager.Core.Providers;
using DependencyManager.Lib;
using DependencyManager.Providers.Default;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;

namespace DependencyManager.Console
{
    [Subcommand(typeof(InstallCommand))]
    [Subcommand(typeof(TestCommand))]
    class Program
    {
        static int Main(string[] args) =>
            CommandLineApplication.Execute<Program>(args);

        private void OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
        }
    }
}
