using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyManager.Core
{
    public class AdministratorRequiredException : Exception
    {
        public AdministratorRequiredException()
            : base("This provider requires administrator requirements.  Please restart the process as administrator.") { }
    }
}
