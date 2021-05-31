﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyManager.Core.Providers
{
    public interface IDependencyConfigurationProvider
    {
        Task<Dictionary<string, object>> GetSoftwareConfigurationAsync();
    }
}
