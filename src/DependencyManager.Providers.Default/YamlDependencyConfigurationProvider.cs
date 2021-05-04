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
        public async Task<Dictionary<object, object>> GetSoftwareConfigurationAsync()
        {
            var yamlPath = Path.Combine(Environment.CurrentDirectory, "dependencies.yaml");

            using var reader = File.OpenText(yamlPath);
            var deserializer = new DeserializerBuilder()
                .Build();

            return deserializer.Deserialize<dynamic>(await reader.ReadToEndAsync());
        }
    }
}
