using Clcrutch.Extensions.DependencyInjection;
using DependencyManager.Core.Providers;
using Microsoft.Win32;
using System.Composition;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace DependencyManager.Providers.Windows
{
    [Export(typeof(IOperatingSystemProvider))]
    [OperatingSystemRequired(OperatingSystems.Windows)]
    public class WindowsOperatingSystemProvider : IOperatingSystemProvider
    {
        /// <summary>
        /// Gets the full path of the given executable filename as if the user had entered this
        /// executable in a shell. So, for example, the Windows PATH environment variable will
        /// be examined. If the filename can't be found by Windows, null is returned.</summary>
        /// <param name="executable"></param>
        /// <returns>The full path if successful, or null otherwise.</returns>
        public Task<string?> GetFullExecutablePathAsync(string executable)
        {
            if (executable.Length >= MAX_PATH)
                throw new ArgumentException($"The executable name '{executable}' must have less than {MAX_PATH} characters.",
                    nameof(executable));

            UpdatePathEnvironmentVariable();

            if (string.IsNullOrEmpty(Path.GetExtension(executable)))
            {
                var fileNames = from e in Environment.GetEnvironmentVariable("PATHEXT")?.Split(';')
                                select $"{executable}{e}";

                foreach (var fileName in fileNames)
                {
                    StringBuilder sb = new(fileName, MAX_PATH);
                    if (PathFindOnPath(sb, null))
                    {
                        return Task.FromResult(sb?.ToString());
                    }
                }

                return Task.FromResult<string?>(null);
            }
            else
            {
                StringBuilder sb = new(executable, MAX_PATH);
                return Task.FromResult(PathFindOnPath(sb, null) ? sb.ToString() : null);
            }
        }

        public Task<bool> IsSuperUserAsync()
        {
            if (OperatingSystem.IsWindows())
            {
                using var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return Task.FromResult(principal.IsInRole(WindowsBuiltInRole.Administrator));
            }

            throw new NotSupportedException();
        }

        public void UpdatePathEnvironmentVariable()
        {
            var scopes = new EnvironmentVariableTarget[]
            {
                EnvironmentVariableTarget.Machine,
                EnvironmentVariableTarget.User
            };

            var paths = (from s in scopes
                         select GetEnvironmentVariable("PATH", s)?.Split(';', StringSplitOptions.RemoveEmptyEntries))
                         .SelectMany(x => x)
                         .Distinct();

            Environment.SetEnvironmentVariable("PATH", string.Join(';', paths), EnvironmentVariableTarget.Process);
        }

        private string? GetEnvironmentVariable(string name, EnvironmentVariableTarget scope, bool preserveVariables = false)
        {
            if (!OperatingSystem.IsWindows())
            {
                throw new PlatformNotSupportedException();
            }

            const string MACHINE_ENVIRONMENT_REGISTRY_KEY_NAME = @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment\";
            const string USER_ENVIRONMENT_REGISTRY_KEY_NAME = "Environment";

            using RegistryKey? userRegistryKey = Registry.CurrentUser.OpenSubKey(USER_ENVIRONMENT_REGISTRY_KEY_NAME);
            using RegistryKey? machineRegistryKey = Registry.LocalMachine.OpenSubKey(MACHINE_ENVIRONMENT_REGISTRY_KEY_NAME);
            RegistryKey? win32RegistryKey = null;
            switch (scope)
            {
                case EnvironmentVariableTarget.Process:
                    return Environment.GetEnvironmentVariable(name);
                case EnvironmentVariableTarget.User:
                    win32RegistryKey = Registry.CurrentUser.OpenSubKey(USER_ENVIRONMENT_REGISTRY_KEY_NAME);
                    break;
                case EnvironmentVariableTarget.Machine:
                    win32RegistryKey = Registry.LocalMachine.OpenSubKey(MACHINE_ENVIRONMENT_REGISTRY_KEY_NAME);
                    break;
            }

            var registryValueOptions = RegistryValueOptions.None;
            if (preserveVariables)
            {
                registryValueOptions = RegistryValueOptions.DoNotExpandEnvironmentNames;
            }

            string? value = null;
            if (win32RegistryKey != null)
            {
                value = (string?)win32RegistryKey.GetValue(name, registryValueOptions);
            }

            return value ?? Environment.GetEnvironmentVariable(name, scope);
        }

        // https://docs.microsoft.com/en-us/windows/desktop/api/shlwapi/nf-shlwapi-pathfindonpathw
        // https://www.pinvoke.net/default.aspx/shlwapi.PathFindOnPath
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        static extern bool PathFindOnPath([In, Out] StringBuilder pszFile, [In] string[]? ppszOtherDirs);

        // from MAPIWIN.h :
        private const int MAX_PATH = 260;
    }
}
