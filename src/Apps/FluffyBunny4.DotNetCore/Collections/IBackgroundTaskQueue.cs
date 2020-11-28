using System;
using System.Threading;
using System.Threading.Tasks;

namespace FluffyBunny4.DotNetCore.Collections
{
    public interface IBackgroundTaskQueue<T> where T : class
    {
        void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem);

        Task<Func<CancellationToken, Task>> DequeueAsync(
            CancellationToken cancellationToken);
    }
}
