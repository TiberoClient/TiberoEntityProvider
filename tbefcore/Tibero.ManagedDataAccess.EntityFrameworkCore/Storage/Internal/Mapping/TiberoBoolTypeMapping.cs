using System;
using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Tibero.EntityFrameworkCore.Storage.Internal.Mapping
{
    public class TiberoBoolTypeMapping : BoolTypeMapping
    {
        public TiberoBoolTypeMapping([NotNull] string storeType, DbType? dbType = null)
          : this(new RelationalTypeMapping.RelationalTypeMappingParameters(new CoreTypeMapping.CoreTypeMappingParameters(typeof(bool), 
              (ValueConverter)null, (ValueComparer)null, (ValueComparer)null, (Func<IProperty, IEntityType, ValueGenerator>)null), 
              storeType, StoreTypePostfix.PrecisionAndScale, dbType, false, new int?(), false, new int?(), new int?()))
        {
        }

        protected TiberoBoolTypeMapping(
          RelationalTypeMapping.RelationalTypeMappingParameters parameters)
          : base(parameters)
        {
        }

        public override RelationalTypeMapping Clone(string storeType, int? size)
        {
            return (RelationalTypeMapping)new TiberoBoolTypeMapping(this.Parameters.WithStoreTypeAndSize(storeType, size, new StoreTypePostfix?()));
        }

        public override CoreTypeMapping Clone(ValueConverter converter)
        {
            return (CoreTypeMapping)new TiberoBoolTypeMapping(this.Parameters.WithComposedConverter(converter));
        }

        public override MethodInfo GetDataReaderMethod()
        {
            return typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod("GetBoolean");
        }

        protected override void ConfigureParameter(DbParameter parameter)
        {
            base.ConfigureParameter(parameter);
        }

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            return (bool)value ? "1" : "0";
        }
    }
}
