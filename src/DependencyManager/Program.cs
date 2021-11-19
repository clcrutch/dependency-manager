using McMaster.Extensions.CommandLineUtils;

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
