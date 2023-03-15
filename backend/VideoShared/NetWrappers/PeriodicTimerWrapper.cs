namespace VideoShared.NetWrappers;

public interface IPeriodicTimerWrapper
{
    Task<bool> WaitForNextTickAsync(CancellationToken cancellationToken);
}

public class PeriodicTimerWrapper : IPeriodicTimerWrapper
{
    private readonly PeriodicTimer _periodicTimer;

    public PeriodicTimerWrapper(PeriodicTimer periodicTimer)
    {
        _periodicTimer = periodicTimer;
    }

    public async Task<bool> WaitForNextTickAsync(CancellationToken cancellationToken)
    {
        return await _periodicTimer.WaitForNextTickAsync(cancellationToken);
    }
}
