using System;
using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Tibero.DataAccess.Client;
using Tibero.DataAccess.Types;
using Tibero.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Tibero.EntityFrameworkCore.Storage.Internal.ValueConversion;
namespace Tibero.EntityFrameworkCore.Storage.Internal.Mapping
{
    public class TiberoDateTimeOffsetTypeMapping : DateTimeOffsetTypeMapping
    {

        /*
          public TiberoDateTimeOffsetTypeMapping(
              [NotNull] string storeType,
              DbType? dbType = System.Data.DbType.DateTimeOffset)
              : base(storeType, dbType)
          {
          }
          */
        public TiberoDateTimeOffsetTypeMapping(
            [NotNull] string storeType,
            DbType? dbType = System.Data.DbType.DateTimeOffset)
            : this(new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(typeof(TiberoTimeStampTZ), new TiberoDateTimeOffsetConverter()),
                    storeType,
                    StoreTypePostfix.PrecisionAndScale,
                    dbType,
                    false,
                    new int?(),
                    false,
                    new int?(),
                    new int?()))
        {
        }
        protected TiberoDateTimeOffsetTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
        }

       
        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new TiberoDateTimeOffsetTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        
        public override CoreTypeMapping Clone(ValueConverter converter)
            => new TiberoDateTimeOffsetTypeMapping(Parameters.WithComposedConverter(converter));
        public override DbParameter CreateParameter(
    [NotNull] DbCommand command,
    [NotNull] string name,
    [CanBeNull] object value,
    bool? nullable = null)
        {

            Check.NotNull<DbCommand>(command, nameof(command));
            //var value3 = (DateTimeOffset)value;
            //var offset = value3.Offset;
            //var value2 = new TiberoTimeStampTZ(value3.Year, value3.Month, value3.Day, value3.Hour, value3.Minute, value3.Second, value3.Millisecond, offset.ToString());
            //TiberoParameter parameter = (TiberoParameter)base.CreateParameter(command, name, value2, nullable);
            TiberoParameter parameter = (TiberoParameter)base.CreateParameter(command, name, value, nullable);
            if (((TiberoCommand)command).BindByName == false)
            {
                ((TiberoCommand)command).BindByName = true;
            }

            return parameter;
        }
        protected override void ConfigureParameter(DbParameter parameter)
        {
            base.ConfigureParameter(parameter);
            ((TiberoParameter)parameter).TiberoDbType = TiberoDbType.TimeStampTZ;
        }
        protected override string SqlLiteralFormatString
        {
            get
            {
                return "TIMESTAMP'{0:yyyy-MM-dd HH:mm:ss.fff zzz}'";
            }
        }

    }
}