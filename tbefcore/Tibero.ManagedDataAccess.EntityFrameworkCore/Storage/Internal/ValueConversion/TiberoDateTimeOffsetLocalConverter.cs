using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Tibero.DataAccess.Types;
namespace Tibero.EntityFrameworkCore.Storage.Internal.ValueConversion
{

    internal class TiberoDateTimeOffsetLocalConverter : ValueConverter<DateTimeOffset, TiberoTimeStampLTZ>
    {
        internal TiberoDateTimeOffsetLocalConverter()
          : base((Expression<Func<DateTimeOffset, TiberoTimeStampLTZ>>)(x => ToTimeStampLTZ(x)), (Expression<Func<TiberoTimeStampLTZ, DateTimeOffset>>)(x => ToDateTimeOffset(x)))
        {
        }
        private static TiberoTimeStampLTZ ToTimeStampLTZ(DateTimeOffset value)
        {
            var converted = new TiberoTimeStampLTZ(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Millisecond);

            return converted;

        }

        private static DateTimeOffset ToDateTimeOffset(TiberoTimeStampLTZ value)
        {

            var converted = new DateTimeOffset(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, (int)value.Millisecond, TiberoTimeStampLTZ.GetLocalTimeZoneOffset());

            return converted;
        }
    }
}
