using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Tibero.EntityFrameworkCore.Migration.Operation
{
    public class TiberoDropDatabaseOperation : MigrationOperation
    {
        
        public virtual string Name { get; [param: NotNull] set; }

       
    }
}
