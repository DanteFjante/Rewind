using Microsoft.Extensions.DependencyInjection;
using Rewind.Middleware;
using Rewind.Store.Internal.Registrations;

namespace Rewind.Store.Builders
{
    public interface IMiddlewareBuilder<TState, TMiddleware>
        where TMiddleware : BaseMiddleware<TState>
    {
        public IMiddlewareBuilder<TState, TMiddleware> SetLifeTime(ServiceLifetime lifetime);
        public IMiddlewareBuilder<TState, TMiddleware> UseDefaultSetup();
        public IMiddlewareBuilder<TState, TMiddleware> SetFactory(Action<IServiceCollection> provider, Func<IServiceProvider, TMiddleware> factory);

        public IMiddlewareBuilder<TState, TMiddleware> SetFactory(Func<IServiceProvider, TMiddleware> factory);

        internal MiddlewareRegistration Build();
    }
}
