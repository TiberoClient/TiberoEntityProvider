using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Update;
using Tibero.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Buffers;

namespace Tibero.EntityFrameworkCore.Update.Internal
{
    public class TiberoUpdateSqlGenerator : UpdateSqlGenerator, ITiberoUpdateSqlGenerator
    {
        /*
         * MS open source 에서는 base가 되는 UpdateSqlGenerator 에서 기본적인 AppendInsertOperation, AppendUpdateOperation은 다음과 같이 동작함.
         * update, insert 에 대해서 out(readoperation) param 이 있으면, 그 때, affectedrow 에 대한 (select read column list where rowcount= 1 and condition 나열) 에 대한 결과도 추가로 들고옴.
         * 그리고 out param 이 없으면 AppendSselectAffectedCountCommand 를 타면서 아무 작업도 안하고 그냥 noresultset state만 반환하는 형식임. 
         * delete 에 대해서는 따로 outparam이 없을테니, 바로 noresultset state 만 반환.
         * 
         * 여기서 유추 할수 있는 점은, insert, update 가 되고나면 outparam에 대한 resultset을 들고 오고, 없으면 그냥 resultset 이 없게 들고온다.
         * 
         * 아래내용들은 PSM 에서 진행되는 내용들로, insert, update 에 대해서 out param 이 있으면 psm 변수에 이를 returning into 절로 바로 담아서 조건문에 rowcount 같은게 없어도 
         * open cursor 할 때 담아놓은 변수를 바로 select 하는 형식이다. 
         * update 시에 out param이 없으면, rowcount 가 1 이라는 정보가 담기도록 cursor 를 open 하는데 이는 그냥 noresultset 으로 줘도 무방할 것 같다. 
         * delete는 out param이 따로 없으므로 rowcount 만 담아주거나 noresultset 을 반환하도록 하면 될 것 같다.
         * batch 작업이 이뤄지면서 여러 resultset 이 넘어와서 tdp 단에서 메모리가 어떻게 관리되는지 모르겠지만, 사용자가 메모리가 너무 커지는 것 같다고 하면 noresultset 으로 바꾸고
         * consume 작업에서는 out param 이 없는 경우 resultset 이 없으므로 concurrency check 가 불가능해진다. 
         */
        private IRelationalTypeMappingSource _typeMapper;
        public TiberoUpdateSqlGenerator([NotNull] UpdateSqlGeneratorDependencies dependencies)
            : base(dependencies)
        {
            this._typeMapper = dependencies.TypeMappingSource;
        }
        public TiberoUpdateSqlGenerator([NotNull] UpdateSqlGeneratorDependencies dependencies,
                                        [NotNull] IRelationalTypeMappingSource typeMappingSource)
            : base(dependencies)
        {
            this._typeMapper = typeMappingSource;
        }
        
        public virtual ResultSetMapping AppendInsertOperationWithCursor(
            StringBuilder commandStringBuilder,
            ModificationCommand command,
            int commandPosition,
            StringBuilder varInPSM,
            ref int cursorIndex)
        {
            /* outparam 이 있을 때만, psm 변수 선언하여 select 절로 cursor open 
             * 없으면 아예 noresultset
             */
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(command, nameof(command));

            /* Inparam, Outparam array 로 구한다.*/
            var InParamOperations = command.ColumnModifications.Where(o => o.IsWrite).ToArray();
            var OutParamOperations = command.ColumnModifications.Where(o => o.IsRead).ToArray();

            /*
             * TODO!!! : 아래 default value 만 넣을 수 있도록 class & method spec 확인해서 구현해야함. 
             */ 
            if (InParamOperations.Count() > 0)
            {
                AppendInsertCommandHeader(commandStringBuilder, command.TableName, command.Schema, InParamOperations);
                AppendValuesHeader(commandStringBuilder, InParamOperations);
                AppendValues(commandStringBuilder, InParamOperations);
            }
            else if (InParamOperations.Count() == 0 && OutParamOperations.Count() > 0)
            {
                /* insert value 할 때 default only 인 경우가 있음....
            * tibero의 경우 모든 테이블에 default value를 넣으면 알아서 default 있는 column 만 처리를 해주긴함. 
            * 문제는 insert 구문을 만들 때 넣고자 하는 테이블의 column 정보를 어떻게 알아야하는지 모르겠어서 일단 
            * returning 하는 column 으로만 기술토록 해놓음. 
            * 
            * 근데 당장 그런 케이스를 발생시키는 경우가 많이 없을 것 같음
            */
                AppendInsertCommandHeader(commandStringBuilder, command.TableName, command.Schema, OutParamOperations);
                AppendValuesHeader(commandStringBuilder, InParamOperations);
                AppendDefaultOnly(commandStringBuilder, OutParamOperations);
            }
            else if(InParamOperations.Count() ==0 && OutParamOperations.Count() == 0)
            {
                throw new NotImplementedException("Insert only default value without returning value cannot be implemented");
            }
            

            /*
             * outParam 있을 때 RETURNING INTO 절로 cursor를 열어야한다. 
             */
            /* For TEST */
            
            //OutParamOperations = (ColumnModification[])InParamOperations.Clone();

            if (OutParamOperations.Length > 0)
            {
                commandStringBuilder.Append(" RETURNING ")
                                    .AppendJoin(OutParamOperations.Select(c => (string.Format("{0}.{1}", this.SqlGenerationHelper.DelimitIdentifier(command.TableName), this.SqlGenerationHelper.DelimitIdentifier(c.ColumnName)))))
                                    .Append(" INTO ")
                                    .AppendJoin(OutParamOperations.Select(c => (string.Format("v{0}_{1}{2}", command.TableName, c.ColumnName, commandPosition))));
                                    
                //AppendReturningClause(commandStringBuilder, OutParamOperations);
                commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator).AppendLine();
             
                commandStringBuilder.Append(string.Format("OPEN :C{0} FOR SELECT ", cursorIndex++))
                                    .AppendJoin(OutParamOperations.Select(c => (string.Format("v{0}_{1}{2}", command.TableName, c.ColumnName, commandPosition))))
                                    .AppendLine(" FROM DUAL;");

                varInPSM.AppendJoin<ColumnModification>((IEnumerable<ColumnModification>)OutParamOperations, (Action<StringBuilder, ColumnModification>)((sb, Cm) => sb.Append(string.Format("v{0}_{1}{2}", command.TableName, Cm.ColumnName, commandPosition)).Append(" ").Append(GetStoreType(Cm)).Append(";")), "\n").AppendLine();

                return ResultSetMapping.LastInResultSet;
            }
            else
            {
                commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator).AppendLine();
                return ResultSetMapping.NoResultSet;
            }
        }
     
    
        public virtual ResultSetMapping AppendUpdateOperationWithCursor(
            StringBuilder commandStringBuilder,
            ModificationCommand command,
            int commandPosition,
            StringBuilder varInPSM,
            ref int cursorIndex)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(command, nameof(command));

            var tableName = command.TableName;
            var schemaName = command.Schema;
            var operations = command.ColumnModifications;

            var InParamOperations = operations.Where(o => o.IsWrite).ToArray();
            var conditionOperations = operations.Where(o => o.IsCondition).ToArray();
            var OutParamOperations = operations.Where(o => o.IsRead).ToArray();

            /* FOR TEST */
            //OutParamOperations = (ColumnModification[])InParamOperations.Clone();

            AppendUpdateCommandHeader(commandStringBuilder, tableName, schemaName, InParamOperations);
            AppendWhereClause(commandStringBuilder, conditionOperations);
            if (OutParamOperations.Length > 0)
            {
                commandStringBuilder.Append(" RETURNING ")
                                    .AppendJoin(OutParamOperations.Select(c => (string.Format("{0}.{1}", this.SqlGenerationHelper.DelimitIdentifier(command.TableName), this.SqlGenerationHelper.DelimitIdentifier(c.ColumnName)))))
                                    .Append(" INTO ")
                                    .AppendJoin(OutParamOperations.Select(c => (string.Format("v{0}_{1}{2}", command.TableName, c.ColumnName, commandPosition))));

                //AppendReturningClause(commandStringBuilder, OutParamOperations);
                commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator).AppendLine();

                commandStringBuilder.Append(string.Format("OPEN :C{0} FOR SELECT ", cursorIndex++))
                                    .AppendJoin(OutParamOperations.Select(c => (string.Format("v{0}_{1}{2}", command.TableName, c.ColumnName, commandPosition))))
                                    .AppendLine(" FROM DUAL;");

                varInPSM.AppendJoin<ColumnModification>((IEnumerable<ColumnModification>)OutParamOperations, (Action<StringBuilder, ColumnModification>)((sb, Cm) => sb.Append(string.Format("v{0}_{1}{2}", command.TableName, Cm.ColumnName, commandPosition)).Append(" ").Append(GetStoreType(Cm)).Append(";")), "\n").AppendLine();

            }
            else
            {
                commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator).AppendLine();
                commandStringBuilder.AppendLine("v_RowCount := SQL%ROWCOUNT;").AppendLine(string.Format("OPEN :C{0} FOR SELECT v_RowCount FROM DUAL;", cursorIndex++));
            }
            
            return ResultSetMapping.LastInResultSet;
        }

        public virtual ResultSetMapping AppendDeleteOperationWithCursor(
          StringBuilder commandStringBuilder,
          ModificationCommand command,
          int commandPosition,
          StringBuilder varInPSM,
          ref int cursorIndex)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(command, nameof(command));

            var tableName = command.TableName;
            var schemaName = command.Schema;
            var operations = command.ColumnModifications;

            var InParamOperations = operations.Where(o => o.IsWrite).ToArray();
            var conditionOperations = operations.Where(o => o.IsCondition).ToArray();
            var OutParamOperations = operations.Where(o => o.IsRead).ToArray();

            /* FOR TEST */
            //OutParamOperations = (ColumnModification[])InParamOperations.Clone();

            /* 그냥 기본 delete문 사용 */
            base.AppendDeleteCommand(commandStringBuilder, tableName, schemaName, conditionOperations);
 
            /* rowcount resultset 반환 */
            commandStringBuilder.AppendLine("v_RowCount := SQL%ROWCOUNT;").AppendLine(string.Format("OPEN :C{0} FOR SELECT v_RowCount FROM DUAL;", cursorIndex++));
           
            return ResultSetMapping.LastInResultSet;
        }
        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        private void AppendReturningClause(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ColumnModification> operations)
        {
            commandStringBuilder
                .AppendLine()
                .Append("RETURNING ")
                .AppendJoin(operations.Select(c => SqlGenerationHelper.DelimitIdentifier(c.ColumnName)));
        }

        public override void AppendNextSequenceValueOperation(StringBuilder commandStringBuilder, string name, string schema)
        {
            commandStringBuilder.Append("SELECT ");
            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, Check.NotNull(name, nameof(name)), schema);
            commandStringBuilder.Append(".NEXTVAL FROM DUAL");
        }

        public override void AppendBatchHeader(StringBuilder commandStringBuilder)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            
            // TODO: Npgsql
        }

        protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, ColumnModification columnModification)
        {
            throw new NotImplementedException();
        }

        protected override void AppendRowsAffectedWhereCondition(StringBuilder commandStringBuilder, int expectedRowsAffected)
        {
            throw new NotImplementedException();
        }
        /*protected override void AppendInsertCommandHeader(StringBuilder commandStringBuilder, string name, string schema, IReadOnlyList<ColumnModification> operations)
        {
            if(operations.Count >0)
                base.AppendInsertCommandHeader(commandStringBuilder, name, schema, operations);
            else
            {
                commandStringBuilder.Append("INSERT INTO ");
                SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, name, schema);
                commandStringBuilder.AppendJoin(operations.Select(c => SqlGenerationHelper.DelimitIdentifier(c.ColumnName)));

            }
        }*/
        protected override void AppendValuesHeader(StringBuilder commandStringBuilder, IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(operations, nameof(operations));

            commandStringBuilder.AppendLine();
            commandStringBuilder.Append("VALUES ");
        }
        protected void AppendDefaultOnly(StringBuilder commandStringBuilder, IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, nameof(DbCommandMethod));
            Check.NotNull(operations, nameof(operations));
            //commandStringBuilder.Append("(").AppendJoin(operations, SqlGenerationHelper, 
            //                                            (sb, o, helper) => {sb.Append("DEFAULT") })
           
            commandStringBuilder.Append("(");
            for(int i=0; i < operations.Count; i++)
            {
                if (i == operations.Count - 1)
                    commandStringBuilder.Append("DEFAULT)");
                else
                    commandStringBuilder.Append("DEFAULT, ");
                
            }
            //string[] a = new[] stri
            //commandStringBuilder.Append("(").AppendJoin();
        }
        public enum ResultsGrouping
        {
            OneResultSet,
            OneCommandPerResultSet
        }

        private string GetStoreType(ColumnModification column)
        {
            StringBuilder storeType = new StringBuilder();

            storeType.Append(_typeMapper.FindMapping(column.Property).StoreType);
            return storeType.ToString();
        }
        
    }
}
