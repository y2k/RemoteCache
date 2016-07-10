using System;

namespace RemoteCache.Common
{
    static class Extensions
    {
        public static R Let<T, R>(this T instance, Func<T, R> f) => f(instance);
    }
}