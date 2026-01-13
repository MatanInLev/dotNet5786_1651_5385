namespace Helpers;

internal class AsyncMutex
{
    // Interlocked works with int, not bool. 
    // 0 = false (not in progress), 1 = true (in progress)
    private int _inProgress = 0;
    // Atomically sets _inProgress to 1 only if it is currently 0.
    // CompareExchange returns the ORIGINAL value:
    // - If it returns 1: It was already in progress -> return true.
    // - If it returns 0: It was free (we just acquired it) -> return false.
    internal bool CheckAndSetInProgress() => Interlocked.CompareExchange(ref _inProgress, 1, 0) == 1;
    // Atomically resets the state to 0 (false).
    internal void UnsetInProgress() => Interlocked.Exchange(ref _inProgress, 0);
}
