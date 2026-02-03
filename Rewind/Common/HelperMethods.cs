using System;
using System.Collections.Generic;
using System.Text;

namespace Rewind.Common
{
    public static class HelperMethods
    {
        public static string StoreName<TState>()
        {
            return typeof(TState).FullName!;
        }
    }
}
