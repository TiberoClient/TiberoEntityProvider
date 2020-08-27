using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Tibero.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using Tibero.EntityFrameworkCore.Metadata.Internal;
using Tibero.EntityFrameworkCore.Metadata;
using Tibero.EntityFrameworkCore.Migration.Operation;
using Tibero.EntityFrameworkCore.Update.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.Logging;

using Microsoft.EntityFrameworkCore.Diagnostics;
using Serilog;

namespace Tibero.EntityFrameworkCore.Migration
{
    public class TiberoMigrationSqlGenerator : MigrationsSqlGenerator
    {
        //Dictionary<>
        string _sequencePrefix = "Seq";
        string _triggerPrefix = "TR";
        bool _underPSM = false;
        IDiagnosticsLogger<DbLoggerCategory.Migrations> _logger;
        public TiberoMigrationSqlGenerator([NotNull] MigrationsSqlGeneratorDependencies dependencies,
            IDiagnosticsLogger<DbLoggerCategory.Migrations> logger = null) 
            : base(dependencies)
        {
            _logger = logger;
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoMigrationSqlGenerator::Constructor");
            }
        }

        public override IReadOnlyList<MigrationCommand> Generate(IReadOnlyList<MigrationOperation> operations, IModel model)
        {
            Check.NotNull(operations, nameof(operations));
            
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoMigrationSqlGenerator::Generate(IReadOnlyList<MigrationOperation>, IModel)");
            }
            var builder = new MigrationCommandListBuilder(Dependencies.CommandBuilderFactory);
            foreach (var operation in operations)
            {
                Generate(operation, model, builder);
            }

            return builder.GetCommandList();
        }
        protected override void Generate(MigrationOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoMigrationSqlGenerator::Generate(MigrationOperation, IModel, MigrationCommandListBuilder)");
            }
            var dropDatabaseOperation = operation as TiberoDropDatabaseOperation;
            if (operation is TiberoCreateDatabaseOperation createDatabaseOperation)
            {
                Generate(createDatabaseOperation, model, builder);
            }
            else if (dropDatabaseOperation != null)
            {
                Generate(dropDatabaseOperation, model, builder);
            }
            else
            {
                base.Generate(operation, model, builder);
            }
        }
        protected virtual void Generate(
            [NotNull] TiberoCreateDatabaseOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            /* migration sql 에서 create database는 선언하지 않을 것임. */
            throw new InvalidOperationException("CANNOT CREATE DATABASE IN MIGRATION TIME");
        }
        protected virtual void Generate(
           [NotNull] TiberoDropDatabaseOperation operation,
           [CanBeNull] IModel model,
           [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            /* migration sql 에서 DROP database는 선언하지 않을 것임. */
            throw new InvalidOperationException("CANNOT DROP DATABASE IN MIGRATION TIME");
        }
        protected override void Generate(
           AddColumnOperation operation,
           IModel model,
           MigrationCommandListBuilder builder,
           bool terminate)
        {
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoMigrationSqlGenerator::Generate(AddColumnOperation, IModel, MigrationCommandListBuilder)");
            }
            /*base를 그대로 타도 무방함 */
            base.Generate(operation, model, builder, terminate);
        }
        protected override void Generate(
           AddForeignKeyOperation operation,
           IModel model,
           MigrationCommandListBuilder builder)
        {
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoMigrationSqlGenerator::Generate(AddForeignKeyOperation, IModel, MigrationCommandListBuilder)");
            }
            /*base를 그대로 타도 무방함 */
            base.Generate(operation, model, builder, terminate:true);
        }
        protected override void Generate(
            AddPrimaryKeyOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoMigrationSqlGenerator::Generate(AddPrimaryKeyOperation, IModel, MigrationCommandListBuilder)");
            }
            /*base를 그대로 타도 무방함 */
            base.Generate(operation, model, builder, terminate: true);
        }

        protected override void Generate(
           RenameColumnOperation operation,
           IModel model,
           MigrationCommandListBuilder builder)
        {
            /* rename 시에 해당 column 이 identity column 이면 고려할게 생김. (사실 identity column 이 한 table 에 한개만 존재한다는 확신만 있으면 sequence 이름을 table 까지만 기술할 텐데. 나중에 확인되면 리팩토링.)
             * 1. 일단 sequence 이름을 바꿔줘야한다. 
             * 2. trigger 의 경우 trigger에서 기술하는 column name을 바꿔줘야하니까 trigger 재생성. 
             * 3. 일단 identity column 의 이름을 바꾸지 말라고 throw를 내고 싶지만 operation을 통해 identity 여부를 확인할 수가 없네...
             * 4. 2,3 과 같이 하려 했으나, sequence, trigger 를 table 이름까지만 선언하여 한 table 에 대해 한개의 identity 컬럼을 가지는 것으로 가정. 따라서 rename은 상관 없음. 
             * 5. rename시에 걸려있는 index 도 이름을 바꿔줘야 하는 줄 알았지만, MS BASE 에서 migration assembly 를 보고 index 도 같이 rename 시킴. 
             * 즉 column rename 시에는 column 이름만 바꿔주면 될 것 같음. 
             */
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoMigrationSqlGenerator::Generate(RenameColumnOperation, IModel, MigrationCommandListBuilder)");
            }
            builder.Append("ALTER TABLE ").Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                   .Append(" RENAME COLUMN ").Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                   .Append(" TO ").Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName))
                   .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            builder.EndCommand();

        }
        protected override void Generate(
          RenameIndexOperation operation,
          IModel model,
          MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoMigrationSqlGenerator::Generate(RenameIndexOperation, IModel, MigrationCommandListBuilder)");
            }
            builder.Append("ALTER INDEX ").Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                   .Append(" RENAME ")
                   .Append(" TO ").Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName))
                   .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            builder.EndCommand();

        }
        protected override void Generate(
          RenameSequenceOperation operation,
          IModel model,
          MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoMigrationSqlGenerator::Generate(RenameSequenceOperation, IModel, MigrationCommandListBuilder)");
            }
            builder.Append("RENAME ").Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                   .Append(" TO ").Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName))
                   .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            builder.EndCommand();

        }
        protected override void Generate(
         RenameTableOperation operation,
         IModel model,
         MigrationCommandListBuilder builder)
        {
            /* test 해봐야할 것. => table 에 identity column 박아놓고, table 이름 바꾸고, 다시 identity column drop 하는 과정에서 과연 어떻게 될까. */
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoMigrationSqlGenerator::Generate(RenameTableOperation, IModel, MigrationCommandListBuilder)");
            }
            builder.Append("ALTER TABLE ").Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                   .Append(" RENAME ")
                   .Append(" TO ").Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName))
                   .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            builder.EndCommand();

        }
        protected override void Generate(
            CreateTableOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoMigrationSqlGenerator::Generate(RenameTableOperation, IModel, MigrationCommandListBuilder)");
            }
            builder
                .Append("CREATE TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                .AppendLine(" (");

            using (builder.Indent())
            {
                for (var i = 0; i < operation.Columns.Count; i++)
                {
                    var column = operation.Columns[i];
                    ColumnDefinition(column, model, builder);

                    if (i != operation.Columns.Count - 1)
                    {
                        builder.AppendLine(",");
                    }
                }

                if (operation.PrimaryKey != null)
                {
                    builder.AppendLine(",");
                    PrimaryKeyConstraint(operation.PrimaryKey, model, builder);
                }

                foreach (var uniqueConstraint in operation.UniqueConstraints)
                {
                    builder.AppendLine(",");
                    UniqueConstraint(uniqueConstraint, model, builder);
                }

                foreach (var foreignKey in operation.ForeignKeys)
                {
                    builder.AppendLine(",");
                    ForeignKeyConstraint(foreignKey, model, builder);
                }

                builder.AppendLine();
            }

            builder.Append(")");

            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                   .EndCommand(false);

            /* identity 확인해서 sequence 랑 trigger 생성해야함 */

            StringBuilder ForIdentity = new StringBuilder();
            bool hasIdentity = false;
            for (var i = 0; i < operation.Columns.Count; i++)
            {
                _underPSM = true;
                AddColumnOperation column = operation.Columns[i];
                if (IsIdentity(column))
                {
                   GenerateIdentity(column, ForIdentity, builder);
                    hasIdentity = true;
                }
                _underPSM = false;
            }
            if (hasIdentity)
            {
                builder.AppendLine("BEGIN");
               

                using (builder.Indent())
                {
                    builder.AppendLine(ForIdentity.ToString());
                }

                builder.Append("END").AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                       .EndCommand(false);
                
            }
        }
        private void GenerateIdentity(AddColumnOperation column, StringBuilder stringBuilder, MigrationCommandListBuilder builder)
        {
            /* sequence 부터 만들자. 
             * Sequence 생성 구문은 다음과 같다. 
             * CREATE SEQUENCE [시퀀스명]
             * INCREMENT BY [증감 숫자] -- 음수도 가능.
             * START WITH [시작 숫자] -- 시작숫자 없으면 default 로 Minvlaue 혹은 Maxvalue
             * NOMINVALUE OR MINVALUE [최소값] -- NOMINVALUE : 디폴트 값을 설정하는 애임. 증가일 때 1, 감소일 때 -1028, MINVALUE : 최소값 설정
             * NOMAXVALUE OR MAXVALUE [최대값] -- NOMAXVALUE : 디폴트 값 설정하는 애임. 증가일 때 1027, 감소일 때 -1, MAXVALUE : 최대값 설정
             * CYCLE OR NOCYCLE -- cycle 여부
             * CACHE OR NOCACHE -- 메모리에 시퀀스 미리 할당할지 말지 결정.
             * 일단 아래 시퀀스는 모두 default 설정을 따르도록 한다.
             */
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoMigrationSqlGenerator::GenerateIdentity");
            }
            string sequenceName = _sequencePrefix + "_" + column.Table;
            string triggerName = _triggerPrefix + "_" + column.Table;
            if (_underPSM)
                stringBuilder.Append("EXECUTE IMMEDIATE '");
            stringBuilder.Append("CREATE SEQUENCE ").Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(sequenceName, column.Schema));
            if (_underPSM)
                stringBuilder.Append("'");
            
            stringBuilder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            
            GenerateTrigger(column, triggerName, sequenceName, stringBuilder, builder);
        }
        private void GenerateTrigger(AddColumnOperation column, string triggerName, string sequenceName, StringBuilder stringbuilder, MigrationCommandListBuilder builder)
        {
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoMigrationSqlGenerator::GenerateTrigger");
            }
            if (_underPSM)
                stringbuilder.Append("EXECUTE IMMEDIATE '");
            stringbuilder.Append("CREATE OR REPLACE TRIGGER ").AppendLine(Dependencies.SqlGenerationHelper.DelimitIdentifier(triggerName, column.Schema))
                         .Append("BEFORE INSERT ON ").AppendLine(Dependencies.SqlGenerationHelper.DelimitIdentifier(column.Table, column.Schema))
                         .AppendLine("FOR EACH ROW").Append("WHEN (new.").Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(column.Name)).AppendLine(" IS NULL)")
                         .AppendLine("BEGIN").Append(":new.").Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(column.Name)).Append(" := ").Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(sequenceName, column.Schema))
                         .Append(".NEXTVAL").AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                         .Append("END").Append(Dependencies.SqlGenerationHelper.StatementTerminator); 
            if (_underPSM)
                stringbuilder.Append("'");
            stringbuilder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

        }
        private void DropIdentity(AlterColumnOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoMigrationSqlGenerator::DropIdentity");
            }
            string sequenceName = _sequencePrefix + "_" + operation.Table;
            string triggerName = _triggerPrefix + "_" + operation.Table;

            /* altercolumn 에서 old column을 확인할 수 없는 경우, 새로운 column 의 성질이 identity 가 아닌 경우 무조건 identity drop을 한번씩 타야하기 때문에
             * 해당 이름으로 sequence 와 trigger가 있는지 확인해서 drop.
             */
            builder.AppendLine("DECLARE").AppendLine("v_cnt INTEGER;").AppendLine("BEGIN");

            using (builder.Indent())
            {
                /* 아래 where 절에는 " 를 사용하여 대소문자 구분해야하는 애들에 대해서 그냥 써줘야 되더라.. 그래서 그렇게함.*/
                builder.Append("SELECT COUNT(*) INTO v_cnt FROM USER_SEQUENCES WHERE SEQUENCE_NAME = '").Append(sequenceName).Append("'").AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                       .AppendLine("IF v_cnt > 0").AppendLine("THEN");
                builder.Append("EXECUTE IMMEDIATE 'DROP SEQUENCE ").Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(sequenceName, operation.Schema)).Append("'")
                       .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                builder.AppendLine("END IF;");
                builder.Append("SELECT COUNT(*) INTO v_cnt FROM ALL_TRIGGERS WHERE TRIGGER_NAME = '").Append(triggerName).Append("'").AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                       .AppendLine("IF v_cnt > 0").AppendLine("THEN");
                builder.Append("EXECUTE IMMEDIATE 'DROP TRIGGER ").Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(triggerName, operation.Schema)).Append("'")
                       .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                builder.AppendLine("END IF;");
            }
            builder.Append("END").AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            builder.EndCommand(false);

        }
        protected override void Generate(
            AlterColumnOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoMigrationSqlGenerator::Generate(AlterColumnOperation, IModel, MigrationCommandListBuilder)");
            }

            bool needIdentity = false;
            var property = FindProperty(model, operation.Schema, operation.Table, operation.Name);
            /*
             * 변경하려는 column 이 ComputedColumn이라면 dropOperation으로 해당 column 에 대해 수행한 후에 새로운 column을 add 해주는 방향으로 진행.
             */
            if (operation.ComputedColumnSql != null)
            {
                var dropColumnOperation = new DropColumnOperation
                {
                    Schema = operation.Schema,
                    Table = operation.Table,
                    Name = operation.Name
                };
                if (property != null)
                {
                    //dropColumnOperation.AddAnnotations(_migrationsAnnotations.ForRemove(property));
                }

                var addColumnOperation = new AddColumnOperation
                {
                    Schema = operation.Schema,
                    Table = operation.Table,
                    Name = operation.Name,
                    ClrType = operation.ClrType,
                    ColumnType = operation.ColumnType,
                    IsUnicode = operation.IsUnicode,
                    MaxLength = operation.MaxLength,
                    IsRowVersion = operation.IsRowVersion,
                    IsNullable = operation.IsNullable,
                    DefaultValue = operation.DefaultValue,
                    DefaultValueSql = operation.DefaultValueSql,
                    ComputedColumnSql = operation.ComputedColumnSql,
                    IsFixedLength = operation.IsFixedLength
                };

                /* 해당 column(property) operation에 annotation(이쯤 오면 annotation은 그냥 성질 같은거라고 보면될듯.) 추가 해서 진행해야지.*/
                addColumnOperation.AddAnnotations(operation.GetAnnotations());

                Generate(dropColumnOperation, model, builder, terminate: false);
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                Generate(addColumnOperation, model, builder, terminate: false);
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                builder.EndCommand(false);

                return;
            }
            /* Computed column이 아닌 경우 일반적인 column으로 만들어야한다. 
             * 1. default value 를 일단 제거 후 그냥 새로운 default 를 확인해서 달아주도록(MS 는 default 성질을 drop add 할 수 있는 것 같아 보임. tibero는 따로 없으니..)
             * 2. identity 인 경우 sequence 로 대체하고 있을테니, identity 의 제거 혹은 생성 경우에 대해서만 sequence 관리.
             * 3. index 의 경우 tibero는 테스트 시 다음과 같이 동작한다. column 성질 변경에 대해서는 index가 그대로 적용됨. column 이 drop되는 경우는 index 도 같이 drop(여러 column으로 묶여도 drop)
             *    따라서 altercolumn에서는 성질 변화(modify)이므로, 따로 index는 건드리지 않는다. column rename 에서는 index도 이름을 같이 변경시켜줘야 할듯함. 
             */

            /* identity 정리 */
            if (IsOldColumnSupported(model))
            {
                if (!IsIdentity(operation) && IsIdentity(operation.OldColumn))
                {
                    /* old column 이 identity column 인데, 새로운 column은 아니라면 drop identity 를 해줘야한다.
                     * 아니라면 기존 identity 를 고대로 사용하면 된다. 
                     */
                    DropIdentity(operation, model, builder);
                }
                else if (IsIdentity(operation) && !IsIdentity(operation.OldColumn))
                {
                    needIdentity = true;
                }
                else if (IsIdentity(operation) && IsIdentity(operation.OldColumn))
                {
                    DropIdentity(operation, model, builder);
                    needIdentity = true;
                }
                //if((operation.DefaultValue == null || operation.DefaultValueSql == null) && (operation.OldColumn.DefaultValue != null || operation.OldColumn.DefaultValueSql != null))
                //    DropDefaultConstraint(operation.Schema, operation.Table, operation.Name, builder);

            }
            else
            {
                if (!IsIdentity(operation))
                    DropIdentity(operation, model, builder);
                else
                {
                    DropIdentity(operation, model, builder);
                    needIdentity = true;
                }
      
                //if(operation.DefaultValue == null && operation.DefaultValueSql == null)
                //    DropDefaultConstraint(operation.Schema, operation.Table, operation.Name, builder);
            }

            /* default constraint 정리 
             * default value의 경우에는 default null 을 계속 alter 해도 상관이 없다. 그러니까 그냥 null 로 바꾸고 추가로 값이 있으면 columndefinition 에서 정의하도록 하자. 
             * 근데 문제는 nullable 을 표현하는 NULL / NOT NULL 은 바뀌기전값과 같으면 에러를 뱉는다. 문제는 columndefinition 전에 값을 조회할 수가 없으니,
             * 그냥 MigrationExecutor 에서 catch 문으로 잡자. 
             */
            DropDefaultConstraint(operation.Schema, operation.Table, operation.Name, builder);

            builder
                .Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" MODIFY ");


            var definitionOperation = new AlterColumnOperation
            {
                Schema = operation.Schema,
                Table = operation.Table,
                Name = operation.Name,
                ClrType = operation.ClrType,
                ColumnType = operation.ColumnType,
                IsUnicode = operation.IsUnicode,
                IsFixedLength = operation.IsFixedLength,
                MaxLength = operation.MaxLength,
                IsRowVersion = operation.IsRowVersion,
                IsNullable = operation.IsNullable,
                DefaultValue = operation.DefaultValue,
                DefaultValueSql = operation.DefaultValueSql,
                ComputedColumnSql = operation.ComputedColumnSql,

                OldColumn = operation.OldColumn
            };

            definitionOperation.AddAnnotations(
                operation.GetAnnotations().Where(
                    a => a.Name != TiberoAnnotationNames.ValueGenerationStrategy
                        && a.Name != TiberoAnnotationNames.Identity));

            ColumnDefinition(
                definitionOperation,
                model,
                builder);

            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            builder.EndCommand(false);

            if (needIdentity)
            {
                _underPSM = true;
                StringBuilder ForIdentity = new StringBuilder();
                var addColumnOperation2 = new AddColumnOperation
                {
                    Schema = operation.Schema,
                    Table = operation.Table,
                    Name = operation.Name,
            
                };
                GenerateIdentity(addColumnOperation2, ForIdentity, builder);

                _underPSM = false;

                builder.AppendLine("BEGIN");

                using (builder.Indent())
                {
                    builder.AppendLine(ForIdentity.ToString());
                }

                builder.Append("END").AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                       .EndCommand(false);

            }
        }
        private static bool IsIdentity(ColumnOperation operation)
        {
            var c = operation.FindAnnotation(TiberoAnnotationNames.ValueGenerationStrategy);
           
            return operation[TiberoAnnotationNames.Identity] != null
               || operation[TiberoAnnotationNames.ValueGenerationStrategy] as TiberoValueGenerationStrategy?
               == TiberoValueGenerationStrategy.IdentityColumn;
        }

        protected virtual void DropDefaultConstraint(
            [CanBeNull] string schema,
            [NotNull] string tableName,
            [NotNull] string columnName,
            [NotNull] MigrationCommandListBuilder builder)
        {
            /* Tibero 에는 default 제약을 drop 하는 방법은 없는 것 같으므로, null 조건을 default 로 가져간다. */
            Check.NotEmpty(tableName, nameof(tableName));
            Check.NotEmpty(columnName, nameof(columnName));
            Check.NotNull(builder, nameof(builder));

            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoMigrationSqlGenerator::DropDefaultConstraint");
            }

            builder.Append("ALTER TABLE ").Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(tableName, schema)).Append(" MODIFY ")
                   .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(columnName)).Append(" DEFAULT NULL").AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                   .EndCommand(false);
        }
        protected void ColumnDefinition(AlterColumnOperation operation, IModel model, MigrationCommandListBuilder builder)
           => ColumnDefinition(
               operation.Schema,
               operation.Table,
               operation.Name,
               operation.ClrType,
               operation.ColumnType,
               operation.IsUnicode,
               operation.MaxLength,
               operation.IsFixedLength,
               operation.IsRowVersion,
               operation.IsNullable,
               operation.DefaultValue,
               operation.DefaultValueSql,
               operation.ComputedColumnSql,
               operation,
               model,
               builder);
        protected override void ColumnDefinition(AddColumnOperation operation, IModel model, MigrationCommandListBuilder builder)
            => ColumnDefinition(
                operation.Schema,
                operation.Table,
                operation.Name,
                operation.ClrType,
                operation.ColumnType,
                operation.IsUnicode,
                operation.MaxLength,
                operation.IsFixedLength,
                operation.IsRowVersion,
                operation.IsNullable,
                operation.DefaultValue,
                operation.DefaultValueSql,
                operation.ComputedColumnSql,
                operation,
                model,
                builder);

        protected override void ColumnDefinition(
            string schema,
            string table,
            string name,
            Type clrType,
            string type,
            bool? unicode,
            int? maxLength,
            bool rowVersion,
            bool nullable,
            object defaultValue,
            string defaultValueSql,
            string computedColumnSql,
            IAnnotatable annotatable,
            IModel model,
            MigrationCommandListBuilder builder)
            => ColumnDefinition(schema, table, name, clrType, type, unicode, maxLength, null,
                rowVersion, nullable, defaultValue, defaultValueSql, computedColumnSql, annotatable, model, builder);

        protected override void ColumnDefinition(
            string schema,
            string table,
            string name,
            Type clrType,
            string type,
            bool? unicode,
            int? maxLength,
            bool? fixedLength,
            bool rowVersion,
            bool nullable,
            object defaultValue,
            string defaultValueSql,
            string computedColumnSql,
            IAnnotatable annotatable,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            var valueGenerationStrategy = annotatable[
                TiberoAnnotationNames.ValueGenerationStrategy] as TiberoValueGenerationStrategy?;

            ColumnDefinition(
                schema,
                table,
                name,
                clrType,
                type,
                unicode,
                maxLength,
                fixedLength,
                rowVersion,
                nullable,
                defaultValue,
                defaultValueSql,
                computedColumnSql,
                valueGenerationStrategy == TiberoValueGenerationStrategy.IdentityColumn,
                annotatable,
                model,
                builder);
        }
        protected virtual void ColumnDefinition(
           [CanBeNull] string schema,
           [NotNull] string table,
           [NotNull] string name,
           [NotNull] Type clrType,
           [CanBeNull] string type,
           bool? unicode,
           int? maxLength,
           bool? fixedLength,
           bool rowVersion,
           bool nullable,
           [CanBeNull] object defaultValue,
           [CanBeNull] string defaultValueSql,
           [CanBeNull] string computedColumnSql,
           bool identity,
           [NotNull] IAnnotatable annotatable,
           [CanBeNull] IModel model,
           [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(clrType, nameof(clrType));
            Check.NotNull(annotatable, nameof(annotatable));
            Check.NotNull(builder, nameof(builder));

            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoMigrationSqlGenerator::ColumnDefinition");
            }

            if (computedColumnSql != null)
            {
                builder
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(name))
                    .Append(" ")
                    .Append(type ?? GetColumnType(schema, table, name, clrType, unicode, maxLength, fixedLength, rowVersion, model))
                    .Append(" AS (")
                    .Append(computedColumnSql)
                    .Append(" )");

                return;
            }

            builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(name))
                   .Append(" ")
                   .Append(type ?? GetColumnType(schema, table, name, clrType, unicode, maxLength, fixedLength, rowVersion, model));

            /*
             * 원래는 identity column 과 default constraint 는 동시에 존재할 수 없다. 근데 sequence 로 대체할 경우는 어떻게 해야할지 생각해봐야함.
             */
            if (identity)
            {
                //identity 는 alter column 에서 처리가 되어야할 것 같다. definition 에서 추가로 해줄 수 있는 건 없음. 
            }
            else
            {
                DefaultValue(defaultValue, defaultValueSql, builder);
            }

            /* nullable 체크. 
             * 문제는 기존에 NULL 이거나 NOT NULL 을 동일하게 기술하면 에러발생함. 
             */
            builder.Append(nullable ? " NULL" : " NOT NULL");
        }
        protected override void ForeignKeyConstraint(
            [NotNull] AddForeignKeyOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoMigrationSqlGenerator::ForeignKeyConstraint");
            }

            if (operation.Name != null)
            {
                builder
                    .Append("CONSTRAINT ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .Append(" ");
            }

            builder
                .Append("FOREIGN KEY (")
                .Append(ColumnList(operation.Columns))
                .Append(") REFERENCES ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.PrincipalTable, operation.PrincipalSchema));

            if (operation.PrincipalColumns != null)
            {
                builder
                    .Append(" (")
                    .Append(ColumnList(operation.PrincipalColumns))
                    .Append(")");
            }
            /* 아래 ON UPDATE & ON DELETE 에서 RESTRICT 도 무시 */
            if (operation.OnUpdate != ReferentialAction.NoAction && operation.OnUpdate != ReferentialAction.Restrict)
            {
                builder.Append(" ON UPDATE ");
                ForeignKeyAction(operation.OnUpdate, builder);
            }

            if (operation.OnDelete != ReferentialAction.NoAction && operation.OnDelete != ReferentialAction.Restrict)
            {
                builder.Append(" ON DELETE ");
                ForeignKeyAction(operation.OnDelete, builder);
            }
        }
        protected override void ForeignKeyAction(ReferentialAction referentialAction, MigrationCommandListBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoMigrationSqlGenerator::ForeignKeyAction");
            }

            switch (referentialAction)
            {
                case ReferentialAction.Restrict:
                    /* Tibero 에서는 Restrict 가 NO ACTION 인듯 함. 
                     * NO ACTION 은 RESTRICT 와 동일한 동작을 하지만 시점이 다르다고함. transaction 기준으로
                     */
                    break;
                case ReferentialAction.Cascade:
                    builder.Append("CASCADE");
                    break;
                case ReferentialAction.SetNull:
                    builder.Append("SET NULL");
                    break;
                case ReferentialAction.SetDefault:
                    builder.Append("SET DEFAULT");
                    break;
                default:
                    Debug.Assert(
                        referentialAction == ReferentialAction.NoAction,
                        "Unexpected value: " + referentialAction);
                    break;
            }

        }
        /*
        protected override void DefaultValue(object defaultValue, string defaultValueSql, MigrationCommandListBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            if(defaultValueSql != null)
            {
                builder.Append(" DEFAULT (").Append(defaultValueSql).Append(")");
            }
            else if (defaultValue != null)
            {
                var typeMapping = Dependencies.TypeMappingSource.GetMappingForValue(defaultValue);

                builder
                    .Append(" DEFAULT ")
                    .Append(typeMapping.GenerateSqlLiteral(defaultValue));
            }
        }*/

        /* Drop Operations */
        protected override void Generate(DropTableOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            /* drop table 할 때, identity 도 drop 해줘야지... 
             * 근데 해당 table 이 drop될 때 걸려있는 trigger는 알아서 삭제됨.
             */

            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoMigrationSqlGenerator::Generate(DropTableOperation, IModel, MigrationCommandListBuilder)");
            }

            string sequenceName = _sequencePrefix + "_" + operation.Name;
            DropSequenceOperation dropSequnece = new DropSequenceOperation()
            {
                Schema = operation.Schema,
                Name = sequenceName
            };

            base.Generate(operation, model, builder, terminate:true);
            base.Generate(dropSequnece, model, builder);
        }
        protected override void Generate(
            DropPrimaryKeyOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoMigrationSqlGenerator::Generate(DropPrimaryKeyOperation, IModel, MigrationCommandListBuilder)");
            }

            base.Generate(operation, model, builder, terminate: true);
        }
        protected override void Generate(DropForeignKeyOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoMigrationSqlGenerator::Generate(DropForeignKeyOperation, IModel, MigrationCommandListBuilder)");
            }
            base.Generate(operation, model, builder, terminate: true);
            
        }
        protected override void Generate(EnsureSchemaOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoMigrationSqlGenerator::Generate(EnsureSchemaOperation, IModel, MigrationCommandListBuilder)");
            }
            /* 음.. schema 가 있는지 그리고 없다면 만들어야하는데,,, HOW? */

        }
        protected override void Generate(
            CreateSequenceOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoMigrationSqlGenerator::Generate(CreateSequenceOperation, IModel, MigrationCommandListBuilder)");
            }

            builder
                .Append("CREATE SEQUENCE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                .Append(" START WITH ")
                .Append(Dependencies.TypeMappingSource.GetMapping(operation.ClrType).GenerateSqlLiteral(operation.StartValue));

            SequenceOptions(operation, model, builder);

            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }
        protected override void Generate(
            DropIndexOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
            => Generate(operation, model, builder, terminate: true);

          protected virtual void Generate(
            [NotNull] DropIndexOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder,
            bool terminate)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoMigrationSqlGenerator::Generate(DropIndexOperation, IModel, MigrationCommandListBuilder)");
            }

            builder
                .Append("DROP INDEX ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));
             
            if (terminate)
            {
                builder
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                    .EndCommand(false);
            }
        }
        protected override void Generate(
           DropColumnOperation operation,
           IModel model,
           MigrationCommandListBuilder builder)
           => Generate(operation, model, builder, terminate: true);

        protected override void Generate(
            DropColumnOperation operation,
            IModel model,
            MigrationCommandListBuilder builder,
            bool terminate)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoMigrationSqlGenerator::Generate(DropColumnOperation, IModel, MigrationCommandListBuilder)");
            }

            DropDefaultConstraint(operation.Schema, operation.Table, operation.Name, builder);
            base.Generate(operation, model, builder, terminate: false);

            if (terminate)
            {
                builder
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                    .EndCommand(false);
            }
        }
        protected override void Generate(
           InsertDataOperation operation,
           IModel model,
           MigrationCommandListBuilder builder)
        {
            /* migration 단계에서 미리 data 를 넣는 경우. 
             * 그냥 PSM 으로 한번에 insert 구문 수행.
             */
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoMigrationSqlGenerator::Generate(InsertDataOperation, IModel, MigrationCommandListBuilder)");
            }
            var sqlBuilder = new StringBuilder();
            foreach(ModificationCommand command in operation.GenerateModificationCommands(model).ToList())
            {
                ((IUpdateSqlGenerator)Dependencies.UpdateSqlGenerator).AppendInsertOperation(sqlBuilder, command, 0);
            }

            builder.AppendLine("BEGIN").AppendLine(sqlBuilder.ToString()).Append("END").AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            builder.EndCommand(false);
        }
        protected override void SequenceOptions(
            string schema,
            string name,
            int increment,
            long? minimumValue,
            long? maximumValue,
            bool cycle,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(increment, nameof(increment));
            Check.NotNull(cycle, nameof(cycle));
            Check.NotNull(builder, nameof(builder));
            
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoMigrationSqlGenerator::SequenceOptions");
            }

            var intTypeMapper = Dependencies.TypeMappingSource.GetMapping(typeof(int));
            var longTypeMapper = Dependencies.TypeMappingSource.GetMapping(typeof(int));
            builder
                .Append(" INCREMENT BY ")
                .Append(intTypeMapper.GenerateSqlLiteral(increment));

            if (minimumValue.HasValue)
            {
                builder
                    .Append(" MINVALUE ")
                    .Append(longTypeMapper.GenerateSqlLiteral(minimumValue.Value));
            }
            else
            {
                builder.Append(" NOMINVALUE");
            }

            if (maximumValue.HasValue)
            {
                builder
                    .Append(" MAXVALUE ")
                    .Append(longTypeMapper.GenerateSqlLiteral(maximumValue.Value));
            }
            else
            {
                builder.Append(" NOMAXVALUE");
            }

            builder.Append(cycle ? " CYCLE" : " NOCYCLE");
        }
    }
}
