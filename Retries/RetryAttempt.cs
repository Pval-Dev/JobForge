namespace JobForge.Retry;

public class RetryAttempt
{
    public Guid JobId { get; private set; }
    public int AttemptNumber { get; private set; }
    public string ErrorMessage { get; private set; }
    public DateTimeOffset FailedAt { get; private set; }
    public DateTimeOffset NextRetryAt { get; private set; }

    private RetryAttempt() { ErrorMessage = null!; }

    public RetryAttempt(Guid jobId, int attemptNumber, string errorMessage, DateTimeOffset nextRetryAt)
    {
        JobId = jobId;
        AttemptNumber = attemptNumber;
        ErrorMessage = errorMessage;
        FailedAt = DateTimeOffset.UtcNow;
        NextRetryAt = nextRetryAt;
    }
}