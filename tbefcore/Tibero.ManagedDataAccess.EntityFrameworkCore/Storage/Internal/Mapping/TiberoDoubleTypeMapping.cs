using System;
using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Tibero.EntityFrameworkCore.Utilities;
using Tibero.DataAccess.Client;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Internal;
namespace Tibero.EntityFrameworkCore.Storage.Internal.Mapping
{
    public class TiberoDoubleTypeMapping : DoubleTypeMapping
    {
        public TiberoDoubleTypeMapping([NotNull] string storeType, [CanBeNull] DbType? dbType = null)
          : base(storeType, dbType)
        {
            
        }

        protected TiberoDoubleTypeMapping(
          RelationalTypeMappingParameters parameters)
          : base(parameters)
        {
        }

        public override RelationalTypeMapping Clone(string storeType, int? size)
        {
            return new TiberoDoubleTypeMapping(this.Parameters.WithStoreTypeAndSize(storeType, size, new StoreTypePostfix?()));
        }

        public override CoreTypeMapping Clone(ValueConverter converter)
        {
            return new TiberoDoubleTypeMapping(this.Parameters.WithComposedConverter(converter));
        }

        public override DbParameter CreateParameter(
         [NotNull] DbCommand command,
         [NotNull] string name,
         [CanBeNull] object value,
         bool? nullable = null)
        {
            /*
             * 알아서 dbtype 이 float 으로 설정되어 있음;;
             * 찾아보니 내부에서 만든 parameter 에서 tiberoparameter value 를 set 할 때, type 이 안정해져있으면 추측해서 함 ;; 신기.
             */
            Check.NotNull<DbCommand>(command, nameof(command));

            TiberoParameter parameter = (TiberoParameter)base.CreateParameter(command, name, value, nullable);

            if (((TiberoCommand)command).BindByName == false)
            {
                ((TiberoCommand)command).BindByName = true;
            }

            return parameter;
        }

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            string nonNullSqlLiteral = base.GenerateNonNullSqlLiteral(value);
            return !nonNullSqlLiteral.Contains("E") && !nonNullSqlLiteral.Contains("e") ? nonNullSqlLiteral + "E0" : nonNullSqlLiteral;
        }
    }
}
