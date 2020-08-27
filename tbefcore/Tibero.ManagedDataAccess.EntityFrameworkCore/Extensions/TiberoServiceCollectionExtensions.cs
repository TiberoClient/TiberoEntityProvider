using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Tibero.EntityFrameworkCore.Utilities;
using Tibero.EntityFrameworkCore.Infrastructure.Internal;
using Tibero.EntityFrameworkCore.Internal;
using Tibero.EntityFrameworkCore.Storage.Internal;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Tibero.EntityFrameworkCore.Metadata.Conventions;
using Tibero.EntityFrameworkCore.Storage.Internal;
using Tibero.EntityFrameworkCore.Migration;
using Tibero.EntityFrameworkCore.Query.ExpressionTranslators.Internal;
using Tibero.EntityFrameworkCore.Query.Sql.Internal;
using Tibero.EntityFrameworkCore.Update.Internal;
using Tibero.EntityFrameworkCore.Storage.Internal;
using Tibero.EntityFrameworkCore.Query.Internal;
using Tibero.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Design;
using Tibero.EntityFrameworkCore.Design.Internal;
namespace Microsoft.Extensions.DependencyInjection
{
    public static class TiberoServiceCollectionExtensions
    {
        /*
         * 지원하는 service class 및 해당 DI 가능한 interface 를 TryAdd 로 엮는 과정인것 같다.
         * 잘 모르겠지만, EF CORE 에서 특정 service 가 필요할 때 해당 객체들을 부루는 것 같다. 
         * 설명상으로는 singleton 개념? 
         */
        public static IServiceCollection AddEntityFrameworkTibero([NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));
            /*
             * TryAdd<TService, TImplementation> 의 기본적인 설명은 Entity Framework Service 인 TService를 구현한 TImplementation을 추가하는 과정.
             * TService : The contract for the service (간략한)
             * TImplementation : The Concrete type that implements the service(구체적인)
             * Func : Factory that will create the service instance.
             * 
             * 아래 서비스들은 dbcontext 를 생성하는 과정에서 EF CORE에서 등록된 서비스 내용을 확인하는 과정에서 에러를 내는 순서대로 등록해놨음.
             * 아마 static class 내의 static 함수라서 한번 불러보고 반환받은 servicecollection을 들고있는 것 같음.
             * 사용방식은 D.I 인거 같은데 정확히는 모름.
             */
            var builder = new EntityFrameworkRelationalServicesBuilder(serviceCollection)
                .TryAdd<IDatabaseProvider, DatabaseProvider<TiberoOptionsExtension>>()
                .TryAdd<ISingletonOptions, ITiberoOptions>(p => p.GetService<ITiberoOptions>())// p는 IServiceProvider, GetSerivce는 System에 정의된 확장메서드. 여기서 singleton 객체를 만드는듯.(여기란 labda식)
                .TryAdd<IRelationalTypeMappingSource, TiberoTypeMappingSource>()
                .TryAdd<IConventionSetBuilder, TiberoConventionSetBuilder>()
                .TryAdd<IRelationalConnection>(p => p.GetService<ITiberoRelationalConnection>())
                .TryAdd<ICompositeMethodCallTranslator, TiberoCompositeMethodCallTranslator>()
                .TryAdd<IMemberTranslator, TiberoCompositeMemberTranslator>()
                .TryAdd<IQuerySqlGeneratorFactory, TiberoQuerySqlGeneratorFactory>()
                .TryAdd<ISqlGenerationHelper, TiberoSqlGenerationHelper>()
                .TryAdd<IModificationCommandBatchFactory, TiberoModificationCommandBatchFactory>()
                 //.TryAdd<IUpdateSqlGenerator, TiberoUpdateSqlGenerator>()
                 .TryAdd<IUpdateSqlGenerator>(p => p.GetService<ITiberoUpdateSqlGenerator>())
                .TryAdd<ISingletonUpdateSqlGenerator>(p => p.GetService<ITiberoUpdateSqlGenerator>())
                .TryAdd<IMigrationsSqlGenerator, TiberoMigrationSqlGenerator>()
                .TryAdd<IHistoryRepository, TiberoHistoryRepository>()
                .TryAdd<IRelationalDatabaseCreator, TiberoDatabaseCreator>()
                .TryAdd<IMigrationsAnnotationProvider, TiberoMigrationsAnnotationProvider>()
                .TryAdd<IMigrationCommandExecutor, TiberoMigrationCommandExecutor>()
                .TryAdd<IQueryCompilationContextFactory, TiberoQueryCompilationContextFactory>()//사실 이건 필요없을 것 같음. 
                .TryAdd<ICompiledQueryCacheKeyGenerator, TiberoCompileQueryCacheKeyGenerator>()
                .TryAdd<IValueGeneratorSelector, TiberoValueGeneratorSelector>()
                .TryAdd<IValueGeneratorCache>(p => p.GetService<ITiberoValueGeneratorCache>())
                .TryAdd<IExecutionStrategyFactory, TiberoExecutionStrategyFactory>()
                .TryAddProviderSpecificServices(b => b
                    .TryAddSingleton<ITiberoValueGeneratorCache, TiberoValueGeneratorCache>()
                    .TryAddSingleton<ITiberoOptions, TiberoOptions>()
                    .TryAddSingleton<ITiberoUpdateSqlGenerator, TiberoUpdateSqlGenerator>()
                    .TryAddSingleton<ITiberoSequenceValueGeneratorFactory, TiberoSequenceValueGeneratorFactory>()
                    .TryAddScoped<ITiberoRelationalConnection, TiberoRelationalConnection>());
            /*
             * 여기 까지가 기본 서비스 등록.
             */
            

            builder.TryAddCoreServices();

            return serviceCollection;
        }
    }
}
