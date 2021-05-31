using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
