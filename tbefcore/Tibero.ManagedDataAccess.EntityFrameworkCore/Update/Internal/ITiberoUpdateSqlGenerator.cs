using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using System.Collections.Generic;
using System.Text;

namespace Tibero.EntityFrameworkCore.Update.Internal
{
    public interface ITiberoUpdateSqlGenerator : IUpdateSqlGenerator, ISingletonUpdateSqlGenerator
    {
        ResultSetMapping AppendInsertOperationWithCursor(
            StringBuilder commandStringBuilder,
            ModificationCommand modificationCommand,
            int commandPosition,
            StringBuilder varInPSM,
            ref int _cursorIndex);
        ResultSetMapping AppendUpdateOperationWithCursor(
           StringBuilder commandStringBuilder,
           ModificationCommand modificationCommand,
           int commandPosition,
           StringBuilder varInPSM,
           ref int _cursorIndex);
        ResultSetMapping AppendDeleteOperationWithCursor(
           StringBuilder commandStringBuilder,
           ModificationCommand modificationCommand,
           int commandPosition,
           StringBuilder varInPSM,
           ref int _cursorIndex);
    }
}
