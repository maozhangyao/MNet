using System;
using System.Linq;
using System.Collections;

namespace MNet.Utils
{
    public static class EnumerableExtensions
    {
#if NET10_0_OR_GREATER
        extension(IEnumerable list)
        {
            public bool IsEmpty()
            {
                if (list == null)
                    return true;
                if (list is Array arr)
                    return arr.Length <= 0;
                if (list is IList l)
                    return l.Count <= 0;
                if (list is string str)
                    return str.Length <= 0;
                if (list is IDictionary dic)
                    return dic.Count <= 0;
                    
                IEnumerator ator = list.GetEnumerator();
                bool flag = ator.MoveNext();

                if (ator is IDisposable disp)
                    disp.Dispose();

                return flag;
            }
            public bool IsNotEmpty()
            {
                return !list.IsEmpty();
            }
        }
#else


        public static bool IsEmpty(this IEnumerable list)
        {
            if (list == null)
                return true;
            if (list is Array arr)
                return arr.Length <= 0;
            if (list is IList l)
                return l.Count <= 0;
            if (list is string str)
                return str.Length <= 0;
            if (list is IDictionary dic)
                return dic.Count <= 0;

            IEnumerator ator = list.GetEnumerator();
            bool flag = ator.MoveNext();

            if (ator is IDisposable disp)
                disp.Dispose();

            return flag;
        }
        public static bool IsNotEmpty(this IEnumerable list)
        {
            return !list.IsEmpty();
        }
#endif
    }
}
