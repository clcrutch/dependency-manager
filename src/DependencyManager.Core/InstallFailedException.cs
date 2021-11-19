namespace DependencyManager.Core
{
    public class InstallFailedException : Exception
    {
        public InstallFailedException()
            : base("The install process has failed") { }
    }
}
