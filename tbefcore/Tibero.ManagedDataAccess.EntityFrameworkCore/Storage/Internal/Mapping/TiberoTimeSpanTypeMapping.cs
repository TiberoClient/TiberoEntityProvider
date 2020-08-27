using System;
using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Tibero.DataAccess.Client;
using Tibero.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ChangeTracking;
namespace Tibero.EntityFrameworkCore.Storage.Internal.Mapping
{
    public class TiberoTimeSpanTypeMapping :TimeSpanTypeMapping
    {
        public TiberoTimeSpanTypeMapping([NotNull] string storeType, [CanBeNull] DbType? dbType = null)
         : base(storeType, dbType)
        {
        }

        protected TiberoTimeSpanTypeMapping(
          RelationalTypeMappingParameters parameters)
          : base(parameters)
        {
        }

        public override RelationalTypeMapping Clone(string storeType, int? size)
        {
            return new TiberoTimeSpanTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size, new StoreTypePostfix?()));
        }

        public override CoreTypeMapping Clone(ValueConverter converter)
        {
            return new TiberoTimeSpanTypeMapping(Parameters.WithComposedConverter(converter));
        }
        protected override void ConfigureParameter(DbParameter parameter)
        {
            base.ConfigureParameter(parameter);
            ((TiberoParameter)parameter).TiberoDbType = TiberoDbType.IntervalDS;
        }
        protected override string GenerateNonNullSqlLiteral(object value)
        {
            TimeSpan timeSpan = (TimeSpan)value;
           
            return string.Format("INTERVAL '{0} {1}:{2}:{3}.{4}' DAY TO SECOND", timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
        }
    }
}
