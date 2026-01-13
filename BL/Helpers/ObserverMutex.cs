namespace PL.Helpers;

/// <summary>
/// A helper class that synchronizes load operations.
/// It ensures that only one load runs at a time,
/// and tracks whether a restart is required if another request arrives.
/// </summary>
internal class ObserverMutex
{
    // Delay added after a load finishes to avoid too many requests in a short time
    private const int DELAY_MILLISECONDS = 100;

    // Indicates whether a load operation is currently running
    private bool _isLoadInProgress = false;

    // Indicates whether a new load request was received during an active load
    private bool _isRestartRequested = false;

    /// <summary>
    /// Checks whether a load is already in progress.
    /// If so, marks that a restart is required.
    /// Otherwise, marks the load as started.
    /// </summary>
    /// <returns>
    /// True if a load is already in progress (restart required),
    /// False if a new load can start immediately.
    /// </returns>
    internal bool CheckAndSetLoadInProgressOrRestartRequired()
    {
        lock (this) // Ensure thread-safe access to shared state
        {
            // A load is already running – request a restart, but do not start a new load now
            // otherwise, this is the first request and the load can start and no restart is needed
            _isRestartRequested = _isLoadInProgress;

            // Ensure the observer's load is in progress
            _isLoadInProgress = true;

            // If restart was requested, another load is currently in progress, so the caller will finish now
            // otherwise, this is the first request and the load can start
            return _isRestartRequested;
        }
    }

    /// <summary>
    /// Marks the current load as finished and checks
    /// whether a restart was requested during the load.
    /// </summary>
    /// <returns>
    /// True if a restart was requested, false otherwise.
    /// </returns>
    internal async Task<bool> UnsetLoadInProgressAndCheckRestartRequested()
    {
        // Small delay to throttle rapid consecutive load requests
        await Task.Delay(DELAY_MILLISECONDS);

        lock (this) // Ensure thread-safe update of shared state
        {
            // Mark the load as completed
            _isLoadInProgress = false;
            // Return whether another load should be started
            // NB. _isRestartRequested will be reset when setting LoadInProgress next time
            return _isRestartRequested;
        }
    }
}
