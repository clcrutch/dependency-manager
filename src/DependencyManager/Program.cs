using McMaster.Extensions.CommandLineUtils;
using Serilog;

namespace DependencyManager
{
    [VersionOptionFromMember(MemberName = nameof(Version))]
    [Subcommand(typeof(InstallCommand))]
    class Program
    {
        public static string? Version =>
            typeof(Program).Assembly.GetName().Version?.ToString();

        static void Main(string[] args) =>
            CommandLineApplication.Execute<Program>(args);

        private void OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
        }
    }
}
