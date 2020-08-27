using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
namespace Tibero.EntityFrameworkCore.Storage.Internal.ValueConversion
{
    internal class TiberoGuidConverter : ValueConverter<Guid, string>
    {
        internal TiberoGuidConverter()
          : base((Expression<Func<Guid, string>>)(x => x.ToString()), (Expression<Func<string, Guid>>)(x => new Guid(x)))
        {

        }
    }
}
