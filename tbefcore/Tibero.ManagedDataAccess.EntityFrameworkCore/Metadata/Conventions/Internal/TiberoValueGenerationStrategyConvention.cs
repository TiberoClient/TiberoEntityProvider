using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Tibero.EntityFrameworkCore.Metadata.Internal;
using System;
using Tibero.EntityFrameworkCore.Metadata;
using Tibero.EntityFrameworkCore.Extensions;
namespace Tibero.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class TiberoValueGenerationStrategyConvention : IModelInitializedConvention, IModelBuiltConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            /* modelInitialize 는 model 의 초기화 apply 
             * 여기서 valuegenerationstrategy 를 identitycolumn 으로 세팅?
             */ 
            
            modelBuilder.Tibero(ConfigurationSource.Convention).ValueGenerationStrategy(TiberoValueGenerationStrategy.IdentityColumn);
           

            return modelBuilder;
        }
        InternalModelBuilder IModelBuiltConvention.Apply(InternalModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                foreach (var property in entityType.GetDeclaredProperties())
                {
                    property.Builder.Tibero(ConfigurationSource.Convention)
                        .ValueGenerationStrategy(property.Tibero().ValueGenerationStrategy);
                }
            }

            return modelBuilder;
        }
    }
}
