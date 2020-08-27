using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Tibero.EntityFrameworkCore.Storage.Internal;
namespace Tibero.EntityFrameworkCore.Storage.Internal
{
    /*
     * database/scheam 생성과 관계된 operation을 수행한는 class 이다. 
     */
    public class TiberoDatabaseCreator:RelationalDatabaseCreator
    {
        private readonly ITiberoRelationalConnection _connection;
        private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;
        public TiberoDatabaseCreator(
            [NotNull] RelationalDatabaseCreatorDependencies dependencies,
            [NotNull] ITiberoRelationalConnection connection,
            [NotNull] IRawSqlCommandBuilder rawSqlCommandBuilder
            )
            :base(dependencies)
        {
            _connection = connection;
            _rawSqlCommandBuilder = rawSqlCommandBuilder;
        }

        /*
         * True if the database exist; otherwise false.
         */ 
        public override bool Exists()
        {
            /*
             * master connection을 통해서 conn.open(errorsExpected:true)를 통해서 open 되어있는지 확인. 
             * 추가로, rawsqlcommandbuilder를 통해 executenonquery 를 통해 select 1 from dual; 등을 날려봐야한다. 
             * 일단, open 만 확인해보는 걸로 하자. 
             */
            var opened = false;

            _connection.Open(true);
            opened = true;

            if (opened)
                return true;
            else
                return false;
        }

        /*
         * Craete the physical database. Does not attempt to populate it with any schema.
         * 딱 database만들기만 하는 작업인듯.
         */
        public override void CreateTables()
        {
            
            base.CreateTables();
        }
        public override bool EnsureCreated()
        {
           
            return base.EnsureCreated();
        }
        public override void Create()
        {
            if (_connection.GetType() == typeof(TiberoRelationalConnection))
                throw new NotImplementedException("User should create Database");
            else
            {
                _connection.Open();
                /* test를 위한 부분임. 여기서 fake connection open 하고 query 수행했다고 가정함 */
            }
                
        }

        /*
         * 데이터베이스에 어떠한 테이블이 있는지 확인하는 메서드. 테이블이 현재 모델에 속해있는지 아닌지 결정하려고 까지 하지는 않음.
         */ 
        protected  override bool HasTables()
        {
            
            //var a = CreateHasTablesCommand();
            //var b = a.ExecuteScalar(_connection);
            //var c = Convert.ToInt32(b);
            //var d = (bool)(c > 0);
            //return d;
            //bool flag = this.Dependencies.ExecutionStrategyFactory.Create().Execute<ITiberoRelationalConnection, bool>(this._connection, (Func<ITiberoRelationalConnection, bool>)(connection => Convert.ToInt32(this.CreateHasTablesCommand().ExecuteScalar((IRelationalConnection)connection)) > 0));
            return (bool)(Convert.ToInt32(CreateHasTablesCommand().ExecuteScalar(_connection)) > 0);
        }

        private IRelationalCommand CreateHasTablesCommand()
        {
            return  _rawSqlCommandBuilder.Build("SELECT count(*) FROM USER_TABLES");
        }
        /*
         * pysical databse 삭제.
         */ 
        public override void Delete()
        {
            throw new NotImplementedException();
        }
    }
}
