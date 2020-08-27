using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Tibero.DataAccess.Types;
namespace Tibero.EntityFrameworkCore.Storage.Internal.ValueConversion
{

    internal class TiberoDateTimeOffsetConverter : ValueConverter<DateTimeOffset, TiberoTimeStampTZ>
    {
        internal TiberoDateTimeOffsetConverter()
          : base((Expression<Func<DateTimeOffset, TiberoTimeStampTZ>>)(x => ToTimeStampTZ(x)), (Expression<Func<TiberoTimeStampTZ, DateTimeOffset>>)(x => ToDateTimeOffset(x)))
        {
        }
        private static TiberoTimeStampTZ ToTimeStampTZ(DateTimeOffset value)
        {
            var converted = new TiberoTimeStampTZ(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Millisecond, value.Offset.ToString());
                         
            return converted;

        }

        private static DateTimeOffset ToDateTimeOffset(TiberoTimeStampTZ value)
        {

            var converted = new DateTimeOffset(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, (int)value.Millisecond, value.GetTimeZoneOffset());

            return converted;
        }
    }
}
