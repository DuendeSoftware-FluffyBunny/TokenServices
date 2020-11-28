using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FluffyBunny4.Models.Internal
{
    /// <summary>
    /// Helpers to deal with tasks.
    /// </summary>
    public static class TaskHelpers
    {
        /// <summary>
        /// Gets or sets if this library's internal tasks can call ConfigureAwait(false).
        /// </summary>
        public static bool CanConfigureAwaitFalse { get; set; } = true;

        /// <summary>
        /// Gets or sets if this library's internal tasks can call <see cref="TaskFactory.StartNew(System.Action)"/>.
        /// </summary>
        public static bool CanFactoryStartNew { get; set; } = true;

        internal static ConfiguredTaskAwaitable ConfigureAwait(this Task task)
            => task.ConfigureAwait(!CanConfigureAwaitFalse);

        internal static ConfiguredTaskAwaitable<TResult> ConfigureAwait<TResult>(this Task<TResult> task)
            => task.ConfigureAwait(!CanConfigureAwaitFalse);
    }
}
