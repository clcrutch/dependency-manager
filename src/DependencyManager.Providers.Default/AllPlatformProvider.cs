﻿using DependencyManager.Core.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyManager.Providers.Default
{
    public class AllPlatformProvider : IPlatformProvider
    {
        public string Name => "all";

        public Task<bool> TestAsync() =>
            Task.FromResult(true);

        public Task<bool> TestAsync(string version) =>
            Task.FromResult(true);
    }
}