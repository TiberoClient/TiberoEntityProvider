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
    public class TiberoGuidTypeMapping : GuidTypeMapping
    {
        public TiberoGuidTypeMapping(
      [NotNull] string storeType,
      [CanBeNull] DbType? dbType = System.Data.DbType.String,
      int? size = null,
      bool fixedLength = false,
      ValueComparer comparer = null,
      StoreTypePostfix? storeTypePostfix = null)
          : this(new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(typeof(Guid), new TiberoGuidConverter()),
                    storeType,
                    StoreTypePostfix.Size,
                    dbType,
                    false,
                    new int?(),
                    true,
                    new int?(),
                    new int?()))
        {

        }

        protected TiberoGuidTypeMapping(
          RelationalTypeMapping.RelationalTypeMappingParameters parameters)
          : base(parameters)
        {
        }

        public override RelationalTypeMapping Clone(string storeType, int? size)
        {
            return (RelationalTypeMapping)new TiberoGuidTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size));
        }

        public override CoreTypeMapping Clone(ValueConverter converter)
        {
            return (CoreTypeMapping)new TiberoGuidTypeMapping(Parameters.WithComposedConverter(converter));
        }

        public override MethodInfo GetDataReaderMethod()
        {
            return typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod("GetString");
        }

        public override DbParameter CreateParameter(
          [NotNull] DbCommand command,
          [NotNull] string name,
          [CanBeNull] object value,
          bool? nullable = null)
        {
            Check.NotNull<DbCommand>(command, nameof(command));
            TiberoParameter parameter = (TiberoParameter)base.CreateParameter(command, name, value, nullable);
            //TiberoParameter tiberoParameter = new TiberoParameter(name, TiberoDbType.Raw, (object)16, ParameterDirection.Input);
            //tiberoParameter.Value = value;
            if (((TiberoCommand)command).BindByName == false)
            {
                ((TiberoCommand)command).BindByName = true;
            }
            return (DbParameter)parameter;
        }

        protected override void ConfigureParameter(DbParameter parameter)
        {
            base.ConfigureParameter(parameter);
            ((TiberoParameter)parameter).TiberoDbType = TiberoDbType.Varchar2;
        }

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            string guidString = ((Guid)value).ToString();

            if (string.IsNullOrEmpty((string)guidString))
                return "NULL";
            return !this.IsUnicode ? "'" + this.EscapeSqlLiteral((string)guidString) + "'" : "N'" + this.EscapeSqlLiteral((string)value) + "'";
            
        }
        protected virtual string EscapeSqlLiteral([NotNull] string literal)
            => Check.NotNull(literal, nameof(literal)).Replace("'", "''");
    }
}
