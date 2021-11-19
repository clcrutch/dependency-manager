using Clcrutch.Linq;
using DependencyManager.Core.Providers;
using System.Composition;
using YamlDotNet.Serialization;

namespace DependencyManager.Providers.Default
{
    [Export(typeof(IDependencyConfigurationProvider))]
    public class YamlDependencyConfigurationProvider : IDependencyConfigurationProvider
    {
        private readonly IEnumerable<IPlatformProvider> platformProviders;
        private readonly IEnumerable<IArchitectureProvider> architectureProviders;

        public YamlDependencyConfigurationProvider(IEnumerable<IPlatformProvider> platformProviders, IEnumerable<IArchitectureProvider> architectureProviders)
        {
            this.platformProviders = platformProviders;
            this.architectureProviders = architectureProviders;
        }

        public async Task<Dictionary<object, object>> GetSoftwareConfigurationAsync()
        {
            var yamlPath = Path.Combine(Environment.CurrentDirectory, "dependencies.yaml");
            var yamlInfo = new FileInfo(yamlPath);

            if (!yamlInfo.Exists)
            {
                throw new FileNotFoundException("dependencies.yaml not found.");
            }

            using var reader = yamlInfo.OpenText();
            var deserializer = new DeserializerBuilder()
                .Build();

            Dictionary<object, object> sections = deserializer.Deserialize<dynamic>(await reader.ReadToEndAsync());
            var relevantSections = await sections
                                    .Where(s => TestIfRelevantAsync(s.Value))
                                    .Select(s => s.Value as IEnumerable<KeyValuePair<object, object>>)
                                    .SelectMany(s => s ?? Enumerable.Empty<KeyValuePair<object, object>>())
                                    .Where(g => g.Key.ToString() != "platform" && g.Key.ToString() != "architecture" && g.Key.ToString() != "version")
                                    .ToArrayAsync();

            return Combine(relevantSections);
        }

        private Dictionary<object, object> Combine(IEnumerable<KeyValuePair<object, object>> groups)
        {
            var @return = new Dictionary<object, object>();

            foreach (var group in groups)
            {
                if (group.Value is Dictionary<object, object> sourceDict)
                {
                    if (@return.ContainsKey(group.Key))
                    {
                        if (@return[group.Key] is Dictionary<object, object> targetDict)
                        {
                            foreach (var key in sourceDict.Keys)
                            {
                                targetDict.Add(key, sourceDict[key]);
                            }
                        }
                    }
                    else
                    {
                        @return.Add(group.Key, group.Value);
                    }
                }
            }

            return @return;
        }

        private async Task<bool> TestIfRelevantAsync(object obj)
        {
            if (obj is Dictionary<object, object> dict)
            {
                var platform = dict["platform"] as string;
                var arch = dict["architecture"] as string;

                var platformProvider = platformProviders.SingleOrDefault(x => x.Name.Equals(platform, StringComparison.OrdinalIgnoreCase));
                var archProvider = architectureProviders.SingleOrDefault(x => x.Name.Equals(arch, StringComparison.OrdinalIgnoreCase));

                if (platformProvider == null || archProvider == null)
                {
                    return false;
                }

                if (dict.ContainsKey("version") && dict["version"] is string version)
                {
                    return (await Task.WhenAll(platformProvider.TestAsync(version), archProvider.TestAsync())).All(x => x);
                }
                else
                {
                    return (await Task.WhenAll(platformProvider.TestAsync(), archProvider.TestAsync())).All(x => x);
                }
            }

            return false;
        }
    }
}
