namespace PL;

/// <summary>
/// A simple mutex helper to prevent concurrent observer callbacks in the PL layer.
/// Stage 7 - This prevents race conditions when multiple BL notifications arrive simultaneously.
/// </summary>
internal class ObserverMutex
{
    // Interlocked works with int, not bool. 
    // 0 = false (not in progress), 1 = true (in progress)
    private int _inProgress = 0;

    /// <summary>
    /// Atomically sets _inProgress to 1 only if it is currently 0.
    /// CompareExchange returns the ORIGINAL value:
    /// - If it returns 1: It was already in progress -> return true.
    /// - If it returns 0: It was free (we just acquired it) -> return false.
    /// </summary>
    internal bool CheckAndSetInProgress() => Interlocked.CompareExchange(ref _inProgress, 1, 0) == 1;

    /// <summary>
    /// Atomically resets the state to 0 (false).
    /// </summary>
    internal void UnsetInProgress() => Interlocked.Exchange(ref _inProgress, 0);
}
