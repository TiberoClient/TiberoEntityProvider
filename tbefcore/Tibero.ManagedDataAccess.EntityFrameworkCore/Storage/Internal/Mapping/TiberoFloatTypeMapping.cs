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
    public class TiberoFloatTypeMapping : FloatTypeMapping
    {
        public TiberoFloatTypeMapping([NotNull] string storeType, [CanBeNull] DbType? dbType = null)
          : base(storeType, dbType)
        {
          
        }

        protected TiberoFloatTypeMapping(
          RelationalTypeMappingParameters parameters)
          : base(parameters)
        {
        }

        public override RelationalTypeMapping Clone(string storeType, int? size)
        {
            return new TiberoFloatTypeMapping(this.Parameters.WithStoreTypeAndSize(storeType, size, new StoreTypePostfix?()));
        }

        public override CoreTypeMapping Clone(ValueConverter converter)
        {
            return new TiberoFloatTypeMapping(this.Parameters.WithComposedConverter(converter));
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
            return "CAST(" + base.GenerateNonNullSqlLiteral(value) + " AS " + this.StoreType + ")";
        }
    }
}
