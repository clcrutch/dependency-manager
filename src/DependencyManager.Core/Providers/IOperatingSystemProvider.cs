namespace DependencyManager.Core.Providers
{
    public interface IOperatingSystemProvider
    {
        Task<string?> GetFullExecutablePathAsync(string executable);
        Task<bool> IsSuperUserAsync();
    }
}
