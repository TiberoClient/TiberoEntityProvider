using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Tibero.EntityFrameworkCore.Infrastructure;
using Tibero.EntityFrameworkCore.Infrastructure.Internal;

namespace Tibero.EntityFrameworkCore.Internal
{
    public class TiberoOptions : ITiberoOptions
    {
        
        
        public void Initialize(IDbContextOptions options)
        {
            var tiberoOptions = options.FindExtension<TiberoOptionsExtension>() ?? new TiberoOptionsExtension();
            Version = tiberoOptions.Version;
            LogPath = tiberoOptions.LogPath;
        }

        public void Validate(IDbContextOptions options)
        {
            var tiberoOptions = options.FindExtension<TiberoOptionsExtension>() ?? new TiberoOptionsExtension();
                       
        }
        public virtual string Version { get; private set; }
        public virtual string LogPath { get; private set; }
    }
}
