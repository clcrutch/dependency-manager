namespace DependencyManager.Core.Providers
{
    public interface IArchitectureProvider
    {
        string Name { get; }

        Task<bool> TestAsync();
    }
}
