using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
namespace Tibero.EntityFrameworkCore.Storage.Internal.ValueConversion
{
    internal class TiberoSByteConverter : ValueConverter<sbyte, short>
    {
        internal TiberoSByteConverter()
          : base((Expression<Func<sbyte, short>>)(x => Convert.ToInt16(x)), (Expression<Func<short, sbyte>>)(x => Convert.ToSByte(x)))
        {
        }
    }
}
