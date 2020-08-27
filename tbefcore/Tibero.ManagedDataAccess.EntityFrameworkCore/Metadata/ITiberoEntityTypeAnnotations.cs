using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata;
namespace Tibero.EntityFrameworkCore.Metadata
{
    public interface ITiberoEntityTypeAnnotations : IRelationalEntityTypeAnnotations
    {
        /// <summary>
        ///     Indicates whether or not the type is mapped to a memory-optimized table.
        /// </summary>
    }
}
