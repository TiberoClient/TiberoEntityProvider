using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
namespace Tibero.EntityFrameworkCore.Storage.Internal.ValueConversion
{
    internal class TiberoUIntConverter : ValueConverter<uint, long>
    {
        internal TiberoUIntConverter()
          : base((Expression<Func<uint, long>>)(x => Convert.ToInt64(x)), (Expression<Func<long, uint>>)(x => Convert.ToUInt32(x)))
        {
        }
       
    }
}
