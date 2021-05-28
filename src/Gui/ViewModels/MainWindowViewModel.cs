using Avalonia.Media.Imaging;
using DependencyManager.Lib;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SystemBitmap = System.Drawing.Bitmap;

namespace DependencyManager.Gui.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        [DllImport("user32.dll")]
        static extern IntPtr LoadImage(IntPtr hinst,
                                       string lpszName,
                                       uint uType,
                                       int cxDesired,
                                       int cyDesired,
                                       uint fuLoad);

        private bool isInstalling = false;

        public Task<bool> AdministratorRequired { get; }
        public Bitmap? Shield { get; }
        public bool IsInstalling
        {
            get => isInstalling;
            set => this.RaiseAndSetIfChanged(ref isInstalling, value);
        }
        public ReactiveCommand<Unit, Unit> YesClick { get; }
        public ReactiveCommand<Unit, Unit> NoClick { get; }

        public MainWindowViewModel()
        {
            if (OperatingSystem.IsWindows())
            {
                var image = LoadImage(IntPtr.Zero, "#106", 1, 16, 16, 0);
                var systemBitmap = SystemBitmap.FromHicon(image);
                using var stream = new MemoryStream();
                systemBitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;
                Shield = new Bitmap(stream);
            }

            YesClick = ReactiveCommand.CreateFromTask(YesClickImplAsync);
            NoClick = ReactiveCommand.Create(NoClickImpl);

            var executor = new Executor();
            AdministratorRequired = executor.RequiresAdministratorAsync();
        }

        private async Task YesClickImplAsync()
        {
            IsInstalling = true;

            var processStartInfo = new ProcessStartInfo
            {
                FileName = Environment.GetCommandLineArgs()
                                        .Skip(1)
                                        .Take(1)
                                        .Single(),
                Arguments = "install",
                WorkingDirectory = Environment.CurrentDirectory,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = true
            };

            if (await AdministratorRequired)
            {
                processStartInfo.Verb = "runAs";
            }

            var process = Process.Start(processStartInfo);
            await (process?.WaitForExitAsync() ?? Task.FromResult(0));

            Environment.Exit(process?.ExitCode ?? 1);
        }

        private void NoClickImpl()
        {
            Environment.Exit(1);
        }
    }
}
