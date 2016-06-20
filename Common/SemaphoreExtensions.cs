using System;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteCache.Common
{
    static class SemaphoreExtensions
    {
        public static async Task<IDisposable> Use(this SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync();
            return new ActionDisposable(() => semaphore.Release());
        }

        class ActionDisposable : IDisposable
        {
            readonly Func<int> callback;

            public ActionDisposable(Func<int> callback) { this.callback = callback; }

            public void Dispose() => callback();
        }
    }
}