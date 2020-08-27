using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Tibero.EntityFrameworkCore.Storage.Internal.ValueConversion
{

    internal class TiberoULongConverter : ValueConverter<ulong, decimal>
    {
        internal TiberoULongConverter()
          : base((Expression<Func<ulong, decimal>>)(x => Convert.ToDecimal(x)), (Expression<Func<decimal, ulong>>)(x => Convert.ToUInt64(x)))
        {
        }
    }
}
