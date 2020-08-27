using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;
using Tibero.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Storage;
using JetBrains.Annotations;
namespace Tibero.EntityFrameworkCore.Migration
{
    public class TiberoHistoryRepository:HistoryRepository
    {
        /* History Table 관련 메서드 들임. 다른 table 에 관한건 여기를 타지 않음. */
        private readonly IRelationalTypeMappingSource _typeMapper;
        public TiberoHistoryRepository([NotNull] HistoryRepositoryDependencies dependencies,
                                       [NotNull] IRelationalTypeMappingSource typeMappingSource
            ):base(dependencies)
        {
            _typeMapper = typeMappingSource;
        }

        /*
         * Schema.Table 이 존재하는지 확인하는 sql을 만드는 메서드.
         */ 
        protected override string ExistsSql
        {
            get
            {
                
                var existSql = new StringBuilder();
                existSql.Append("SELECT TABLE_NAME FROM ");

                var stringTypeMapper = _typeMapper.GetMapping(typeof(string));

                /*TableSchema : schema that contains history table, if null default schema should be used*/
                if (this.TableSchema != null)
                    existSql.AppendLine("ALL_TABLES ");
                else
                    existSql.AppendLine("USER_TABLES ");

                existSql.AppendLine("WHERE TABLE_NAME = " + stringTypeMapper.GenerateSqlLiteral(this.TableName));

                if (this.TableSchema != null)
                    existSql.AppendLine("AND OWNER = " + stringTypeMapper.GenerateSqlLiteral(this.TableSchema));

                return existSql.ToString();

            }
        }
        public override string GetCreateScript()
        {
            return base.GetCreateScript();
        }
        /*
         * CREATE 구문을 실행할 때, 존재하는 Schema.Table 이 없으면 수행할 script 를 구성하는 메서드.
         * TIBERO 는 PSM 구문을 주로 사용.
         * desc) SQL script that will create the history table if and only if it does not already exist.
         */
        public override string GetCreateIfNotExistsScript()
        {
            var stringMapper = _typeMapper.GetMapping(typeof(string));

            var createIfNot = new StringBuilder();

            /* GetCraeteScript 구문을 확인할 필요가 있음 */
            createIfNot.AppendLine("DECLARE").AppendLine("v_Cnt INTEGER;").AppendLine("BEGIN").AppendLine("SELECT COUNT(*) INTO v_Cnt FROM DUAL WHERE EXISTS (" + ExistsSql + ")")
                       .AppendLine("IF v_Cnt = 0 THEN").Append(this.GetCreateScript()).AppendLine("END IF;").AppendLine("END;");

            return createIfNot.ToString();
        }
        /*
         * 위의 GetCreateIfNotExistsScript 는 HISTORY TABLE 자체를 만드는 메서드라면,
         * 이 메서드는 그 table 안에 특정 migration id 에 해당 하는 data 가 있는지 확인해서 SQL BLOCK 을 시작하는 구문이다. 
         * 따라서 존재하는지 확인하는 SQL BLOCK 을 생성.
         */
        public override string GetBeginIfExistsScript(string migrationId)
        {
            var stringMapper = _typeMapper.GetMapping(typeof(string));

            var beginIfExist = new StringBuilder();

            beginIfExist.AppendLine("DECLARE").AppendLine("v_Cnt INTEGER;").AppendLine("BEGIN").Append("SELECT COUNT(*) INTO v_Cnt FROM ")
                        .Append(this.SqlGenerationHelper.DelimitIdentifier(this.TableName, this.TableSchema))
                        .Append(" WHERE ").Append(this.SqlGenerationHelper.DelimitIdentifier(this.MigrationIdColumnName)).Append(" = ")
                        .AppendLine(stringMapper.GenerateSqlLiteral(migrationId)).AppendLine("IF v_Cnt = 1 THEN");
            return beginIfExist.ToString();
        }
        public override bool Exists()
        {
            var c = base.Exists();
            
            return c;
        }
        public override string GetBeginIfNotExistsScript(string migrationId)
        {
            var stringMapper = _typeMapper.GetMapping(typeof(string));

            var beginIfExist = new StringBuilder();

            beginIfExist.AppendLine("DECLARE").AppendLine("v_Cnt INTEGER;").AppendLine("BEGIN").Append("SELECT COUNT(*) INTO v_Cnt FROM ")
                        .Append(this.SqlGenerationHelper.DelimitIdentifier(this.TableName, this.TableSchema))
                        .Append(" WHERE ").Append(this.SqlGenerationHelper.DelimitIdentifier(this.MigrationIdColumnName)).Append(" = ")
                        .AppendLine(stringMapper.GenerateSqlLiteral(migrationId)).AppendLine("IF v_Cnt = 0 THEN");
            return beginIfExist.ToString();
        }
        /*
         * SQL BLOCK 을 끝내는 메서드.
         */ 
        public override string GetEndIfScript()
        {
            return new StringBuilder().AppendLine("END IF").AppendLine("END").ToString();
        }

        protected override bool InterpretExistsResult([NotNull] object value)
        {
            return (value != null);
        }
        public override IReadOnlyList<HistoryRow> GetAppliedMigrations()
        {
            var c = base.GetAppliedMigrations();
            var b = c.Select(t => t.MigrationId);
           
            return c;

            
        }
        public override string GetInsertScript(HistoryRow row)
        {
            return base.GetInsertScript(row);
        }

    }
}
