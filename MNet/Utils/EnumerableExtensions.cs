using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace MNet.Utils
{
    public static class EnumerableExtensions
    {

#if NET10_0_OR_GREATER
        extension(IEnumerable list)
        {
            public bool IsEmpty()
            {
                return list == null || !list.OfType<object>().Any();
            }
            public bool IsNotEmpty()
            {
                return !list.IsEmpty();
            }
        }
#else
        public static bool IsEmpty(this IEnumerable list)
        {
            return list == null || !list.OfType<object>().Any();
        }
        public static bool IsNotEmpty(this IEnumerable list)
        {
            return !list.IsEmpty();
        }
#endif
    }
}
