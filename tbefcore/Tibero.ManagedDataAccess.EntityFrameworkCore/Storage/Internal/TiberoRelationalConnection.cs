using System.Data.Common;
using System.Linq;
using System;
using System.Net.Security;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tibero.EntityFrameworkCore.Infrastructure.Internal;
using Tibero.DataAccess.Client;

namespace Tibero.EntityFrameworkCore.Storage.Internal
{
    public class TiberoRelationalConnection : RelationalConnection, ITiberoRelationalConnection
    {
        //ProvideClientCertificatesCallback ProvideClientCertificatesCallback { get; }
      

        /// <summary>
        ///     Indicates whether the store connection supports ambient transactions
        /// </summary>
        protected override bool SupportsAmbientTransactions => true;

        public TiberoRelationalConnection([NotNull] RelationalConnectionDependencies dependencies)
            : base(dependencies)
        {
          
            var npgsqlOptions =
                dependencies.ContextOptions.Extensions.OfType<TiberoOptionsExtension>().FirstOrDefault();

        }
      
        protected override DbConnection CreateDbConnection()
        {
            
            var conn = new TiberoConnection(ConnectionString);
            //if (ProvideClientCertificatesCallback != null)
            //    conn.ProvideClientCertificatesCallback = ProvideClientCertificatesCallback;
            //if (RemoteCertificateValidationCallback != null)
            //    conn.UserCertificateValidationCallback = RemoteCertificateValidationCallback;
            return conn;
        }

        public ITiberoRelationalConnection CreateMasterConnection()
        {
            
            /*
            var adminDb = Dependencies.ContextOptions.FindExtension<TiberoOptionsExtension>()?.AdminDatabase
                          ?? "postgres";

            var csb = new TiberoConnectionStringBuilder(ConnectionString)
            {
                Database = adminDb,
                Pooling = false
            };
            var masterConn = ((TiberoConnection)DbConnection).CloneWith(csb.ToString());
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseNpgsql(masterConn);
            */
            var optionsBuilder = new DbContextOptionsBuilder();
            return new TiberoRelationalConnection(Dependencies.With(optionsBuilder.Options));
        }
    }
}
