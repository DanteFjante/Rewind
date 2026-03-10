using Microsoft.Extensions.DependencyInjection;
using Rewind.Base.Store.Interface;
using Rewind.Store.Builders;
using Rewind.Store.Internal.Builders;

namespace Rewind.Store
{
    public static class StoreExtensionMethods
    {
        public static IServiceCollection AddDispatcher(this IServiceCollection services, Func<IDispatcherBuilder, IDispatcherBuilder> dispatcherBuilderFactory)
        {
            DispatcherBuilder builder = new DispatcherBuilder();
            builder = (DispatcherBuilder) dispatcherBuilderFactory(builder);
            builder.BuildFactory(services);
            return services;
        }
    }
}
