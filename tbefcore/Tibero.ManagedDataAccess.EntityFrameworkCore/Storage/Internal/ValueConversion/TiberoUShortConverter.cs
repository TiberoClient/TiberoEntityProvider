using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
namespace Tibero.EntityFrameworkCore.Storage.Internal.ValueConversion
{
    internal class TiberoUShortConverter : ValueConverter<ushort, int>
    {
        internal TiberoUShortConverter()
          : base((Expression<Func<ushort, int>>)(x => Convert.ToInt32(x)), (Expression<Func<int, ushort>>)(x => Convert.ToUInt16(x)))
        {
        }
    }
}
