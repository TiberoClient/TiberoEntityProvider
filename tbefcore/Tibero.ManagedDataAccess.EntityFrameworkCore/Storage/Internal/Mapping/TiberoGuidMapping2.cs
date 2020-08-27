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
using System.Reflection;
using Tibero.EntityFrameworkCore.Storage.Internal.ValueConversion;
namespace Tibero.EntityFrameworkCore.Storage.Internal.Mapping
{
    public class TiberoGuidTypeMapping2 : GuidTypeMapping
    {
        public TiberoGuidTypeMapping2(
          [NotNull] string storeType,
          [CanBeNull] DbType? dbType = System.Data.DbType.Binary,
          int? size = null,
          bool fixedLength = false,
          ValueComparer comparer = null,
          StoreTypePostfix? storeTypePostfix = null)
          : this(new RelationalTypeMapping.RelationalTypeMappingParameters(new CoreTypeMapping.CoreTypeMappingParameters(typeof(Guid), (ValueConverter)null, comparer, (ValueComparer)null, (Func<IProperty, IEntityType, ValueGenerator>)null), storeType, StoreTypePostfix.None, dbType, false, size, fixedLength, new int?(), new int?()))
        {
        }

        protected TiberoGuidTypeMapping2(
          RelationalTypeMapping.RelationalTypeMappingParameters parameters)
          : base(parameters)
        {
        }

        public override RelationalTypeMapping Clone(string storeType, int? size)
        {
            return (RelationalTypeMapping)new TiberoGuidTypeMapping2(this.Parameters.WithStoreTypeAndSize(storeType, size, new StoreTypePostfix?()));
        }

        public override CoreTypeMapping Clone(ValueConverter converter)
        {
            return (CoreTypeMapping)new TiberoGuidTypeMapping2(this.Parameters.WithComposedConverter(converter));
        }

        public override MethodInfo GetDataReaderMethod()
        {
            return typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod("GetInt32");
        }

        public override DbParameter CreateParameter(
          [NotNull] DbCommand command,
          [NotNull] string name,
          [CanBeNull] object value,
          bool? nullable = null)
        {
            Check.NotNull<DbCommand>(command, nameof(command));
            TiberoParameter tiberoParameter = new TiberoParameter(name, TiberoDbType.Raw, (object)16, ParameterDirection.Input);
            tiberoParameter.Value = value;
            return (DbParameter)tiberoParameter;
        }

        protected override void ConfigureParameter(DbParameter parameter)
        {
            base.ConfigureParameter(parameter);
            ((TiberoParameter)parameter).TiberoDbType = TiberoDbType.Raw;
        }

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            byte[] byteArray = ((Guid)value).ToByteArray();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("'");
            foreach (byte num in byteArray)
                stringBuilder.Append(num.ToString("X2", (IFormatProvider)CultureInfo.InvariantCulture));
            stringBuilder.Append("'");
            return stringBuilder.ToString();
        }
    }
}