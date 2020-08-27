using System;
using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using System.Text;
using Tibero.DataAccess.Client;
using Tibero.EntityFrameworkCore.Utilities;
namespace Tibero.EntityFrameworkCore.Storage.Internal.Mapping
{
    class TiberoShortTypeMapping : ShortTypeMapping
    {
        public TiberoShortTypeMapping(
          [NotNull] string storeType,
          [CanBeNull] DbType? dbType
          )
          : base(storeType, dbType)
        {

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
    }
}
