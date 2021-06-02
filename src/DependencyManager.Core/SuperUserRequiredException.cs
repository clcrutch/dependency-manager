using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyManager.Core
{
    public class SuperUserRequiredException : Exception
    {
        public SuperUserRequiredException()
            : base("This provider requires super user requirements.  Please restart the process as super user.") { }
    }
}
