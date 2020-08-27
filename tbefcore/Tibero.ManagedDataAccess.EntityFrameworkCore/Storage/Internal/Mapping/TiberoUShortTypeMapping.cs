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
    class TiberoUShortTypeMapping : UShortTypeMapping
    {
        public TiberoUShortTypeMapping(
          [NotNull] string storeType,
          [CanBeNull] DbType? dbType
          )
          : this(new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(typeof(ushort), new TiberoUShortConverter()),
                    storeType,
                    StoreTypePostfix.PrecisionAndScale,
                    dbType,
                    false,
                    new int?(),
                    false,
                    new int?(),
                    new int?()))
        {
            /* 위의 parameter 를 재생성해서 넣어줄 때 CoreTypeMappingParameter 를 ushort 로 제대로 설정해줘야한다. 그렇지 않으면 
             * ChangeTracking.Internal.ChangeDetector.DeteChanges 에서 에러남.
             * 그리고 converter 등록안해주면, TDP 에서 에러남. 정확한 이유는 나중에 찾아야함 시간이 없음 ..
             */ 
        }
        protected TiberoUShortTypeMapping(
      RelationalTypeMappingParameters parameters)
      : base(parameters)
        {
        }

        public override RelationalTypeMapping Clone(string storeType, int? size)
        {
            return new TiberoUShortTypeMapping(this.Parameters.WithStoreTypeAndSize(storeType, size, new StoreTypePostfix?()));
        }

        public override CoreTypeMapping Clone(ValueConverter converter)
        {
            return new TiberoUShortTypeMapping(Parameters.WithComposedConverter(converter));
        }

        public override MethodInfo GetDataReaderMethod()
        {
            /* unsigend 라서 data 뽑을 때 int32 범위까지 고려해야함 */
            return typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod("GetInt32");
        }

        public override DbParameter CreateParameter(
          [NotNull] DbCommand command,
          [NotNull] string name,
          [CanBeNull] object value,
          bool? nullable = null)
        {
            /*
             * INT Type Mapping 이라서 생성시에 DbType.Int32 로 세팅할 것임. 
             * 따라서 base.CreateParameter 에서 Dbtype 이 int32 로 세팅되어 생성되고, 
             * TiberoParameter 로 typecast 되면서 DbType을 보고 TiberoDbType 도 int32 로 세팅됨. 
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
            ((TiberoParameter)parameter).TiberoDbType = TiberoDbType.Int32;
        }
    }
}