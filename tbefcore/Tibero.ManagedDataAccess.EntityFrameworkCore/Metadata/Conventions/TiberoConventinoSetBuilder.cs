using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.Extensions.DependencyInjection;
using Tibero.EntityFrameworkCore.Metadata.Conventions.Internal;
using Tibero.EntityFrameworkCore.Utilities;
using Tibero.EntityFrameworkCore.Metadata.Conventions.Internal;

namespace Tibero.EntityFrameworkCore.Metadata.Conventions
{
    public class TiberoConventionSetBuilder : RelationalConventionSetBuilder
    {
        /* RelationalConventionSetBuilder
         * Data Provider 에 대한 ConventionSet 을 만드는 Service 이다. 
         * Data Provider 는 이 service 를 반드시 구현해야한다.
         * Service lifetime 은 scoped 되어 있기 때문에 DbContext 하나당 하나 생성되고, thread-safe 하게 구현하지 않아도 된다.
         * 
         * ConvetionSet 
         * model 을 build 하기 위해 사용되는 convention 들이다.
         */ 
        public TiberoConventionSetBuilder([NotNull] RelationalConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
            
        }



        public static ConventionSet Build()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkTibero()
                .AddDbContext<DbContext>(o => o.UseTibero("Data Source=."))
                .BuildServiceProvider();
            
            using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<DbContext>())
                {
                    return ConventionSet.CreateConventionSet(context);
                }
            }
        }
        public override ConventionSet AddConventions(ConventionSet conventionSet)
        {
            Check.NotNull(conventionSet, nameof(conventionSet));

            base.AddConventions(conventionSet);

            var valueGenerationStrategyConvention = new TiberoValueGenerationStrategyConvention();
            conventionSet.ModelInitializedConventions.Add(valueGenerationStrategyConvention);
            conventionSet.ModelInitializedConventions.Add(new RelationalMaxIdentifierLengthConvention(128));
                      
            ValueGeneratorConvention valueGeneratorConvention = new TiberoValueGeneratorConvention();
            ReplaceConvention(conventionSet.BaseEntityTypeChangedConventions, valueGeneratorConvention);

            ReplaceConvention(conventionSet.PrimaryKeyChangedConventions, valueGeneratorConvention);

            ReplaceConvention(conventionSet.ForeignKeyAddedConventions, valueGeneratorConvention);

            ReplaceConvention(conventionSet.ForeignKeyRemovedConventions, valueGeneratorConvention);


            conventionSet.ModelBuiltConventions.Add(valueGenerationStrategyConvention);
                                      

            conventionSet.PropertyAnnotationChangedConventions.Add((TiberoValueGeneratorConvention)valueGeneratorConvention);


            return conventionSet;
        }
    }
}
