using Microsoft.Extensions.Options;
using Rewind.Redux.Middleware;
using Rewind.Redux.Store.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rewind.Redux.Store.Internal
{
    internal class StoreFactory<TState> : IStoreFactory<TState>
    {
        private Dictionary<Type, Func<BaseMiddleware<TState>>> _middlewares;
        private Dictionary<Type, Func<object>> _options;
        private Dictionary<Type, Func<object>> _services;

        public StoreFactory()
        {
            _middlewares = new();
            _options = new();
            _services = new();
        }

        public void Setup(
            Dictionary<Type, Func<BaseMiddleware<TState>>> middlewares,
            Dictionary<Type, Func<object>> options,
            Dictionary<Type, Func<object>> services)
        {

            _middlewares = middlewares;
            _options = options;
            _services = services;
        }

        public TMiddleware? GetMiddleware<TMiddleware>() 
            where TMiddleware : BaseMiddleware<TState>
        {
            if (_middlewares.TryGetValue(typeof(TMiddleware), out var baseMW))
            {
                return baseMW as TMiddleware;
            }
            return null;
        }

        public TOptions? GetOptions<TOptions>() 
            where TOptions : class
        {
            if (_options.TryGetValue(typeof(IOptionsMonitor<TOptions>), out var opt))
            {
                if (opt is IOptionsMonitor<TOptions>)
                {
                    var o = (opt as IOptionsMonitor<TOptions>)!.Get(HelperMethods.StoreName<TState>());
                    return o;
                }
            }
            return null;
        }

        public IOptionsMonitor<TOptions>? GetOptionsMonitor<TOptions>()
            where TOptions : class
        {
            if (_options.TryGetValue(typeof(IOptionsMonitor<TOptions>), out var opt))
            {
                if (opt is IOptionsMonitor<TOptions>)
                {
                    return opt as IOptionsMonitor<TOptions>;
                }
            }
            return null;
        }

        public TService? GetService<TService>() 
            where TService : class
        {
            if (_services.TryGetValue(typeof(TService), out var srv))
            {
                return srv as TService;
            }
            return null;
        }
    }
}
