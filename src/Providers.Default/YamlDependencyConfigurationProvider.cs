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
            var yamlPath = SearchForDepenencyYaml();

            using var reader = File.OpenText(yamlPath);
            var deserializer = new DeserializerBuilder()
                .Build();

            return deserializer.Deserialize<dynamic>(await reader.ReadToEndAsync());
        }

        private string SearchForDepenencyYaml()
        {
            var currentDirectory = new DirectoryInfo(Environment.CurrentDirectory);

            while (currentDirectory != null)
            {
                if (currentDirectory.GetFiles("dependencies.yaml").Any())
                {
                    return Path.Combine(currentDirectory.FullName, "dependencies.yaml");
                }
                else if (currentDirectory.GetDirectories(".git").Any())
                {
                    throw new IOException("dependency.yaml not found.");
                }

                currentDirectory = currentDirectory.Parent;
            }

            throw new IOException("dependency.yaml not found.");
        }
    }
}
