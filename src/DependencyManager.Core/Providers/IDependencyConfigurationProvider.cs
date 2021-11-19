namespace DependencyManager.Core.Providers
{
    public interface IDependencyConfigurationProvider
    {
        Task<Dictionary<object, object>> GetSoftwareConfigurationAsync();
    }
}
