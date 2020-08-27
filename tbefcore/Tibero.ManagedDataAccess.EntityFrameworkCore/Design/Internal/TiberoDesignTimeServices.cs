using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Tibero.EntityFrameworkCore.Scaffolding.Internal;
using Tibero.EntityFrameworkCore.Storage.Internal;

namespace Tibero.EntityFrameworkCore.Design.Internal
{
    public class TiberoDesignTimeServices : IDesignTimeServices
    {
        public virtual void ConfigureDesignTimeServices([NotNull] IServiceCollection serviceCollection)
            => serviceCollection
                .AddSingleton<IRelationalTypeMappingSource, TiberoTypeMappingSource>()
                .AddSingleton<IDatabaseModelFactory, TiberoDatabaseModelFactory>()
                .AddSingleton<IProviderConfigurationCodeGenerator, TiberoConfigurationCodeGenerator>()
                .AddSingleton<IAnnotationCodeGenerator, TiberoAnnotationCodeGenerator>();
                //.TryAddSingleton<ITiberoOptions, TiberoOptions>();
    }
}
