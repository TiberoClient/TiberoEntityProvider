using System;
using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using Tibero.DataAccess.Client;
using Tibero.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Tibero.EntityFrameworkCore.Storage.Internal.ValueConversion;
namespace Tibero.EntityFrameworkCore.Storage.Internal.Mapping
{
    class TiberoULongTypeMapping : LongTypeMapping
    {
        public TiberoULongTypeMapping(
          [NotNull] string storeType,
          [CanBeNull] DbType? dbType
          )
          : this(new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(typeof(ulong), new TiberoULongConverter()),
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
        protected TiberoULongTypeMapping(
      RelationalTypeMappingParameters parameters)
      : base(parameters)
        {
        }

        public override RelationalTypeMapping Clone(string storeType, int? size)
        {
            return new TiberoULongTypeMapping(this.Parameters.WithStoreTypeAndSize(storeType, size, new StoreTypePostfix?()));
        }

        public override CoreTypeMapping Clone(ValueConverter converter)
        {
            return new TiberoULongTypeMapping(Parameters.WithComposedConverter(converter));
        }

        public override MethodInfo GetDataReaderMethod()
        {
            /* unsigend 라서 data 뽑을 때 Decimal로 change */
            return typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod("GetDecimal");
        }

        public override DbParameter CreateParameter(
          [NotNull] DbCommand command,
          [NotNull] string name,
          [CanBeNull] object value,
          bool? nullable = null)
        {
            /* 여기 들어온 순간, DBType과 TiberoDbType은 Decimal로 셋.
             * 등록해준 converter 에서 value 변경도 해줄 것임. 
             */

            Check.NotNull<DbCommand>(command, nameof(command));

            TiberoParameter parameter = (TiberoParameter)base.CreateParameter(command, name, value, nullable);

            if (((TiberoCommand)command).BindByName == false)
            {
                ((TiberoCommand)command).BindByName = true;
            }

            return parameter;
        }
        protected override void ConfigureParameter(DbParameter parameter)
        {
            /* ConfigureParameter 는 CreateParameter 하면서 마지막에 타기 때문에 여기서 필요한 정보 재설정. */
            base.ConfigureParameter(parameter);
            ((TiberoParameter)parameter).TiberoDbType = TiberoDbType.Decimal;
        }
    }
}