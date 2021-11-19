namespace DependencyManager.Core.Providers
{
    public interface IPlatformProvider
    {
        string Name { get; }

        Task<bool> TestAsync();
        Task<bool> TestAsync(string version);
    }
}
