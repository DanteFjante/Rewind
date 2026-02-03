using System;
using System.Collections.Generic;
using System.Text;

namespace Rewind.Redux
{
    public static class HelperMethods
    {
        public static string StoreName<TState>()
        {
            return typeof(TState).FullName!;
        }
    }
}
