using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Tibero.EntityFrameworkCore.Utilities;
using Tibero.DataAccess.Client;
using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text;

namespace Tibero.EntityFrameworkCore.Storage.Internal.Mapping
{
    public class TiberoByteTypeMapping : ByteTypeMapping
    {
        public TiberoByteTypeMapping(
            [NotNull] string storeType,
            DbType? dbType = null)
            :base(storeType, dbType)
        {

        }
        protected TiberoByteTypeMapping(RelationalTypeMappingParameters parameters)
            :base(parameters)
        {

        }
        public override RelationalTypeMapping Clone(string storeType, int? size)
           => new TiberoByteTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size));
        public override CoreTypeMapping Clone(ValueConverter converter)
           => new TiberoByteTypeMapping(Parameters.WithComposedConverter(converter));

        public override DbParameter CreateParameter(
          [NotNull] DbCommand command,
          [NotNull] string name,
          [CanBeNull] object value,
          bool? nullable = null)
        {
            Check.NotNull<DbCommand>(command, nameof(command));
            var c = base.CreateParameter(command, name, value, nullable);
            TiberoParameter parameter = (TiberoParameter)c;
          
            if (((TiberoCommand)command).BindByName == false)
            {
                ((TiberoCommand)command).BindByName = true;
            }
            return (DbParameter)parameter;
        }

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("CAST (").Append(base.GenerateNonNullSqlLiteral(value)).Append(" AS ").Append(StoreType).Append(")");

            return builder.ToString();
        }
    }
}
