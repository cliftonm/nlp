using System;

namespace Calais
{
    /// <summary>
    /// Useful extension methods for creating contracts inside methods.
    /// </summary>
    public static class Predicates
    {
        internal static void Require<T>(this T obj, Func<T, bool> pred, Exception ex)
        {
            if (!pred(obj))
            {
                throw ex;
            }
        }

        internal static void Ensure<T>(this T obj, Func<T, bool> pred, Exception ex)
        {
            if (!pred(obj))
            {
                throw ex;
            }
        }
    }
}