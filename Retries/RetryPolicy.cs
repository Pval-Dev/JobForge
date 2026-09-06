namespace JobForge.Retry;

public sealed class RetryPolicy
{
    public int MaxAttempts { get; } = 5;
    public int BaseDelaySeconds { get; } = 5;
    public int BackoffMultiplier { get; } = 2;

    public bool CanRetry(int currentAttempts)
        => currentAttempts >= 0 && currentAttempts < MaxAttempts;

    public int CalculateDelay(int attemptNumber)
    {
        if (attemptNumber <= 0) throw new ArgumentOutOfRangeException(nameof(attemptNumber));
        double delay = BaseDelaySeconds * Math.Pow(BackoffMultiplier, attemptNumber - 1);
        return checked((int)delay);
    }
}