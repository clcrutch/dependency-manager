using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyManager.Core
{
    public class InstallFailedException : Exception
    {
        public InstallFailedException()
            : base("The install process has failed") { }
    }
}
