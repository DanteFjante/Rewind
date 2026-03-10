using Rewind.Base.Dispatcher.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rewind.Store.Internal.Registrations
{
    public class ReducerRegistration
    {
        public required IReducer Reducer { get; set; }
        public required Func<IServiceProvider, IReducerExecutor> ExecutorFactory { get; set; }
    }
}
