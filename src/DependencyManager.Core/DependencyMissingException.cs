namespace DependencyManager.Core
{
    public class DependencyMissingException : Exception
    {
        public DependencyMissingException(params string[] missingDependencies)
            : base($"The following dependencies are missing: {string.Join(", ", missingDependencies)}")
        {
        }
    }
}
