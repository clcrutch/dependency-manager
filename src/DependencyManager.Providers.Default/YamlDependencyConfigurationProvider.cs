using Clcrutch.Linq;
using DependencyManager.Core.Providers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DependencyManager.Providers.Default
{
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

            using var reader = File.OpenText(yamlPath);
            var deserializer = new DeserializerBuilder()
                .Build();

            Dictionary<object, object> sections = deserializer.Deserialize<dynamic>(await reader.ReadToEndAsync());
            var relevantSections = await sections
                                    .Where(s => TestIfRelevantAsync(s.Value))
                                    .Select(s => s.Value as IEnumerable<KeyValuePair<object, object>>)
                                    .SelectMany(s => s)
                                    .ToArrayAsync();

            return Combine(relevantSections);
        }

        private Dictionary<object, object> Combine(IEnumerable<KeyValuePair<object, object>> groups) =>
            (from g in groups
             where g.Key.ToString() != "platform" && g.Key.ToString() != "architecture"
             select g).ToDictionary(x => x.Key as object, x => x.Value);

        private async Task<bool> TestIfRelevantAsync(object obj)
        {
            if (obj is Dictionary<object, object> dict)
            {
                var platform = dict["platform"] as string;
                var arch = dict["architecture"] as string;

                var platformProvider = platformProviders.Single(x => x.Name.Equals(platform, StringComparison.OrdinalIgnoreCase));
                var archProvider = architectureProviders.Single(x => x.Name.Equals(arch, StringComparison.OrdinalIgnoreCase));

                if (dict.ContainsKey("version"))
                {
                    var version = dict["version"] as string;

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
