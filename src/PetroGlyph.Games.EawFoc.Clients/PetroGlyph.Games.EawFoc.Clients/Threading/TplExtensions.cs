using System;
using System.Threading;
using System.Threading.Tasks;
using Validation;

namespace PetroGlyph.Games.EawFoc.Clients.Threading
{
    // From https://github.com/microsoft/vs-threading
    internal static class TplExtensions
    {
        public static readonly Task<bool> TrueTask = Task.FromResult(true);

        public static readonly Task<bool> FalseTask = Task.FromResult(false);

        internal static Task<bool> ToTask(this WaitHandle handle, int timeout = Timeout.Infinite,
            CancellationToken cancellationToken = default)
        {
            Requires.NotNull(handle, nameof(handle));

            // Check whether the handle is already signaled as an optimization.
            // But even for WaitOne(0) the CLR can pump messages if called on the UI thread, which the caller may not
            // be expecting at this time, so be sure there is no message pump active by controlling the SynchronizationContext.
            using (NoMessagePumpSyncContext.Default.Apply())
            {
                if (handle.WaitOne(0))
                {
                    return TrueTask;
                }

                if (timeout == 0)
                {
                    return FalseTask;
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            var tcs = new TaskCompletionSource<bool>();

            // Arrange that if the caller signals their cancellation token that we complete the task
            // we return immediately. Because of the continuation we've scheduled on that task, this
            // will automatically release the wait handle notification as well.
            var cancellationRegistration =
                cancellationToken.Register(
                    state =>
                    {
                        var (taskCompletionSource, token) =
                            (Tuple<TaskCompletionSource<bool>, CancellationToken>)state!;
                        taskCompletionSource.TrySetCanceled(token);
                    },
                    Tuple.Create(tcs, cancellationToken));

            RegisteredWaitHandle callbackHandle = ThreadPool.RegisterWaitForSingleObject(
                handle,
                (state, timedOut) => ((TaskCompletionSource<bool>)state!).TrySetResult(!timedOut),
                tcs,
                timeout,
                true);

            // It's important that we guarantee that when the returned task completes (whether cancelled, timed out, or signaled)
            // that we release all resources.
            if (cancellationToken.CanBeCanceled)
            {
                // We have a cancellation token registration and a wait handle registration to release.
                // Use a tuple as a state object to avoid allocating delegates and closures each time this method is called.
                tcs.Task.ContinueWith(
                    (_, state) =>
                    {
                        var tuple = (Tuple<RegisteredWaitHandle, CancellationTokenRegistration>)state!;
                        tuple.Item1.Unregister(null); // release resources for the async callback
                        tuple.Item2.Dispose(); // release memory for cancellation token registration
                    },
                    Tuple.Create(callbackHandle, cancellationRegistration),
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
            }
            else
            {
                // Since the cancellation token was the default one, the only thing we need to track is clearing the RegisteredWaitHandle,
                // so do this such that we allocate as few objects as possible.
                tcs.Task.ContinueWith(
                    (_, state) => ((RegisteredWaitHandle)state!).Unregister(null),
                    callbackHandle,
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
            }

            return tcs.Task;
        }
    }
}