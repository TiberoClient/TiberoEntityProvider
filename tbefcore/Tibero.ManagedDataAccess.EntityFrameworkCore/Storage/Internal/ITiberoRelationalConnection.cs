using Microsoft.EntityFrameworkCore.Storage;

namespace Tibero.EntityFrameworkCore.Storage.Internal
{
    public interface ITiberoRelationalConnection : IRelationalConnection
    {
        ITiberoRelationalConnection CreateMasterConnection();
    }
}
