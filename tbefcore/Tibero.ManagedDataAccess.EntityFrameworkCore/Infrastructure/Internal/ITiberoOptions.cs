using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Tibero.EntityFrameworkCore.Infrastructure.Internal
{
    public interface ITiberoOptions : ISingletonOptions
    {
        string Version { get; }

        string LogPath { get; }
    }
}
