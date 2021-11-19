using Clcrutch.Extensions.DependencyInjection;
using DependencyManager.Core.Providers;
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

        // https://docs.microsoft.com/en-us/windows/desktop/api/shlwapi/nf-shlwapi-pathfindonpathw
        // https://www.pinvoke.net/default.aspx/shlwapi.PathFindOnPath
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        static extern bool PathFindOnPath([In, Out] StringBuilder pszFile, [In] string[]? ppszOtherDirs);

        // from MAPIWIN.h :
        private const int MAX_PATH = 260;
    }
}
