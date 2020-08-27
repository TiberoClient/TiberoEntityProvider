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
    public class TiberoDateTimeTypeMapping : DateTimeTypeMapping
    {

        public TiberoDateTimeTypeMapping(
            [NotNull] string storeType,
            DbType? dbType = null)
            : base(storeType, dbType)
        {
        }


        protected TiberoDateTimeTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
        }


        protected override void ConfigureParameter(DbParameter parameter)
        {
            base.ConfigureParameter(parameter);

        }
        public override DbParameter CreateParameter(
    [NotNull] DbCommand command,
    [NotNull] string name,
    [CanBeNull] object value,
    bool? nullable = null)
        {

            Check.NotNull<DbCommand>(command, nameof(command));

            TiberoParameter parameter = (TiberoParameter)base.CreateParameter(command, name, value, nullable);

            if (((TiberoCommand)command).BindByName == false)
            {
                ((TiberoCommand)command).BindByName = true;
            }

            return parameter;
        }
       
        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new TiberoDateTimeTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size));

       
        public override CoreTypeMapping Clone(ValueConverter converter)
            => new TiberoDateTimeTypeMapping(Parameters.WithComposedConverter(converter));

        
        protected override string SqlLiteralFormatString
        { /* 사실 date column 에 timestatmp 써도 먹긴함. */
            get
            {
                if(this.DbType == System.Data.DbType.DateTime)
                    return "TO_TIMESTAMP('{0:yyyy-MM-dd HH:mm:ss.fffffff}', 'YYYY-MM-DD HH24:MI:SS:FF7')";
                if (this.DbType == System.Data.DbType.Date)
                    return "TO_DATE('{0:yyyy-MM-dd}', 'YYYY-MM-DD')";
                return "TO_TIMESTAMP('{0:yyyy-MM-dd HH:mm:ss.fffffff}', 'YYYY-MM-DD HH24:MI:SS:FF7')";
            }
        }
    }
}
